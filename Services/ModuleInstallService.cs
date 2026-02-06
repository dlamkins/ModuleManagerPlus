using Blish_HUD;
using Blish_HUD.Modules;
using Flurl.Http;
using ModuleManagerPlus.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleManagerPlus.Services {
    internal class ModuleInstallService {

        private static readonly Logger Logger = Logger.GetLogger<ModuleInstallService>();

        /// <summary>
        /// Finds the installed <see cref="ModuleManager"/> that matches the given module's namespace.
        /// </summary>
        public ModuleManager FindInstalledModule(Data.Module module) {
            return GameService.Module.Modules
                .FirstOrDefault(m => string.Equals(m.Manifest.Namespace, module.Namespace, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines the install state of a module by comparing against currently loaded modules.
        /// </summary>
        public ModuleInstallState GetInstallState(Data.Module module) {
            var installed = FindInstalledModule(module);

            if (installed == null) {
                return ModuleInstallState.NotInstalled;
            }

            var latestRelease = module.Releases?
                .Where(r => !r.IsPrerelease)
                .OrderByDescending(r => r.TypedVersion)
                .FirstOrDefault();

            if (latestRelease != null && latestRelease.TypedVersion > installed.Manifest.Version) {
                return ModuleInstallState.UpdateAvailable;
            }

            return ModuleInstallState.Installed;
        }

        /// <summary>
        /// Gets the modules directory by examining an existing module's physical path.
        /// </summary>
        private string GetModulesDirectory() {
            var anyModule = GameService.Module.Modules.FirstOrDefault();

            if (anyModule != null) {
                return Path.GetDirectoryName(anyModule.DataReader.PhysicalPath);
            }

            Logger.Warn("Could not determine modules directory — no modules are loaded.");
            return null;
        }

        /// <summary>
        /// Downloads and installs a module from the given release.
        /// </summary>
        public async Task<(bool Success, string Error)> InstallModule(Data.Module module, Release release, IProgress<string> progress = null) {
            string modulesDir = GetModulesDirectory();
            if (modulesDir == null) {
                return (false, "Could not determine modules directory.");
            }

            string moduleName = $"{module.Namespace}_{release.Version}.bhm";
            string fullPath = Path.Combine(modulesDir, moduleName);

            if (File.Exists(fullPath)) {
                return (false, $"Module already exists at {fullPath}.");
            }

            try {
                progress?.Report("Downloading module...");
                byte[] downloadedModule = await release.DownloadUrl.GetBytesAsync();

                progress?.Report("Saving module...");
                File.WriteAllBytes(fullPath, downloadedModule);

                Logger.Info($"Module saved to '{fullPath}'.");

                progress?.Report("Registering module...");
                var newModule = GameService.Module.RegisterPackedModule(fullPath);

                if (newModule == null) {
                    // Registration failed — clean up the downloaded file.
                    TryDeleteFile(fullPath);
                    return (false, "Module registration failed.");
                }

                progress?.Report("");
                Logger.Info($"Module '{module.Name}' v{release.Version} installed successfully.");
                return (true, string.Empty);
            } catch (Exception ex) {
                Logger.Error(ex, $"Failed to install module '{module.Name}'.");
                TryDeleteFile(fullPath);
                return (false, $"Install failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing module to a new release version.
        /// </summary>
        public async Task<(bool Success, string Error)> UpdateModule(Data.Module module, Release release, IProgress<string> progress = null) {
            var existing = FindInstalledModule(module);

            if (existing == null) {
                return await InstallModule(module, release, progress);
            }

            bool wasEnabled = existing.Enabled;

            if (wasEnabled) {
                progress?.Report("Disabling current version...");
                existing.Disable();
            }

            string modulesDir = GetModulesDirectory();
            if (modulesDir == null) {
                return (false, "Could not determine modules directory.");
            }

            string moduleName = $"{module.Namespace}_{release.Version}.bhm";
            string fullPath = Path.Combine(modulesDir, moduleName);

            try {
                progress?.Report("Downloading update...");
                byte[] downloadedModule = await release.DownloadUrl.GetBytesAsync();

                progress?.Report("Saving update...");
                File.WriteAllBytes(fullPath, downloadedModule);

                progress?.Report("Removing old version...");
                existing.DeleteModule();

                progress?.Report("Registering update...");
                var newModule = GameService.Module.RegisterPackedModule(fullPath);

                if (newModule == null) {
                    TryDeleteFile(fullPath);
                    return (false, "Module registration failed after update.");
                }

                if (wasEnabled) {
                    GameService.Module.ModuleStates.Value[module.Namespace].Enabled = true;
                    GameService.Settings.Save();
                }

                progress?.Report("");
                Logger.Info($"Module '{module.Name}' updated to v{release.Version}.");
                return (true, string.Empty);
            } catch (Exception ex) {
                Logger.Error(ex, $"Failed to update module '{module.Name}'.");
                return (false, $"Update failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Uninstalls a module by disabling and deleting it.
        /// </summary>
        public (bool Success, string Error) UninstallModule(Data.Module module) {
            var existing = FindInstalledModule(module);

            if (existing == null) {
                return (false, "Module is not installed.");
            }

            try {
                Logger.Info($"Uninstalling module '{module.Name}'...");
                existing.DeleteModule();
                Logger.Info($"Module '{module.Name}' uninstalled.");
                return (true, string.Empty);
            } catch (Exception ex) {
                Logger.Error(ex, $"Failed to uninstall module '{module.Name}'.");
                return (false, $"Uninstall failed: {ex.Message}");
            }
        }

        private void TryDeleteFile(string path) {
            try {
                if (File.Exists(path)) {
                    File.Delete(path);
                }
            } catch (Exception ex) {
                Logger.Warn(ex, $"Failed to clean up file '{path}'.");
            }
        }
    }
}

using System.Reflection;

namespace Photino.NET;

public static partial class PhotinoWindowExtensions
{
    extension(PhotinoWindow window)
    {
        /// <summary>
        /// Sets the icon file for the native window title bar from an embedded resource.
        /// The resource file is extracted to a temporary file, and its path is then set as the icon.
        /// </summary>
        /// <remarks>
        /// This only works on Windows and Linux.
        /// The resource file is expected to be embedded in the assembly from the `wwwroot` folder, and the provided namespace is used to locate the resource.
        /// </remarks>
        /// <returns>
        /// Returns the current <see cref="PhotinoWindow"/> instance.
        /// </returns>
        /// <param name="resourceFileName">The name of the embedded resource file (e.g., "favicon.ico").</param>
        /// <param name="resourceNamespace">
        /// The namespace in which the embedded resource is located (e.g., "MyApp" or "MyCompany.MyApp").
        /// This allows for specifying the custom namespace where the resource is embedded.
        /// </param>
        public PhotinoWindow SetIconFile(string resourceFileName, string resourceNamespace)
        {
            var iconPath = window.ExtractEmbeddedResourceToTempFile(resourceFileName, resourceNamespace);
            return iconPath != null ? window.SetIconFile(iconPath) : window;
        }

        /// <summary>
        /// Extracts an embedded resource from the assembly to a temporary file.
        /// </summary>
        /// <remarks>
        /// The resource is expected to be located within the provided namespace and under the `wwwroot` folder.
        /// This method will write the resource to a temporary file and return its path.
        /// </remarks>
        /// <returns>
        /// The path to the temporary file containing the extracted resource, or <c>null</c> if the resource was not found.
        /// </returns>
        /// <param name="fileName">The name of the embedded resource file (e.g., "favicon.ico").</param>
        /// <param name="resourceNamespace">
        /// The namespace where the embedded resource is located (e.g., "MyApp" or "MyCompany.MyApp").
        ///
        /// The method expects the resource to be in the `wwwroot` folder of the provided namespace.
        /// </param>
        private string? ExtractEmbeddedResourceToTempFile(string fileName, string resourceNamespace)
        {
            string resourceName = $"{resourceNamespace}.wwwroot.{fileName}";

            Assembly assembly = Assembly.GetExecutingAssembly();

            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                window.Log($"Resource '{fileName}' couldn't be found in namespace '{resourceNamespace}'");
                return null;
            }

            string tempFile = Path.Combine(Path.GetTempPath(), fileName);

            using FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            resourceStream.CopyTo(fileStream);

            return tempFile;
        }
    }
}

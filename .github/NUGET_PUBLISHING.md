# NuGet Package Publishing Guide

This repository uses GitHub Actions to automatically build and publish NuGet packages to NuGet.org.

## Prerequisites

1. **NuGet API Key**: You need to add your NuGet API key as a GitHub secret.
   - Go to [NuGet.org](https://www.nuget.org/account/apikeys) and create an API key
   - In your GitHub repository, go to Settings ? Secrets and variables ? Actions
   - Click "New repository secret"
   - Name: `NUGETORGRAPIDEXDATAAPIKEY`
   - Value: Your NuGet API key
   - Click "Add secret"

## Version Management

**Important:** The workflow automatically uses the version defined in `RapidexAssemblyInfo.cs`:

```csharp
public static class RapidexAssembly
{
    public const string VERSION = "0.2.0.0002";  // <-- This version is used
}
```

### Before Publishing

1. Update the version in `RapidexAssemblyInfo.cs`
2. Commit and push the change
3. Trigger the workflow (see methods below)

## Publishing Methods

### Method 1: Automatic Publishing via Git Tags (Recommended)

This is the recommended way to publish packages. The workflow uses the version from `RapidexAssemblyInfo.cs`:

```bash
# 1. Update version in RapidexAssemblyInfo.cs
# Example: VERSION = "0.2.0.3"

# 2. Commit the change
git add RapidexAssemblyInfo.cs
git commit -m "Bump version to 0.2.0.3"
git push

# 3. Create and push a version tag (any format works, version from file will be used)
git tag v0.2.0.3
git push origin v0.2.0.3
```

The workflow will automatically:
1. Extract version from `RapidexAssemblyInfo.cs`
2. Build the solution
3. Run tests
4. Create NuGet packages with the correct version
5. Publish to NuGet.org
6. Create a GitHub release with the packages attached

### Method 2: Manual Publishing via GitHub UI

1. Go to your repository on GitHub
2. Click on "Actions" tab
3. Select "Publish NuGet Packages" workflow
4. Click "Run workflow" button
5. Choose options:
   - **Use version from RapidexAssemblyInfo.cs**: `true` (default, recommended)
   - **Version number**: Only used if the checkbox above is unchecked
6. Click "Run workflow"

#### Manual Version Override (Not Recommended)

If you need to override the version from the file:
1. Uncheck "Use version from RapidexAssemblyInfo.cs"
2. Enter the desired version number (e.g., 0.2.0.3)
3. Click "Run workflow"

## Version Format

The version in `RapidexAssemblyInfo.cs` uses the format: `MAJOR.MINOR.PATCH.BUILD`

Example: `0.2.0.0002`
- Major: 0
- Minor: 2
- Patch: 0
- Build: 0002

This version is used as-is for NuGet packages.

## Workflow Details

### Version Extraction

The workflow extracts the version using:
```bash
VERSION=$(grep -oP 'VERSION = "\K[^"]+' RapidexAssemblyInfo.cs)
```

This reads the version directly from the file, ensuring consistency across:
- Assembly version
- NuGet package version
- File version
- Informational version

### What Gets Published

The workflow publishes three NuGet packages:
1. **Rapidex.Data** - Core ORM framework
2. **Rapidex.Data.SqlServer** - SQL Server provider
3. **Rapidex.Data.PostgreServer** - PostgreSQL provider

All packages use the same version from `RapidexAssemblyInfo.cs`.

### Build Steps

1. **Checkout** - Gets the code from the repository
2. **Extract Version** - Reads version from `RapidexAssemblyInfo.cs`
3. **Setup .NET** - Installs .NET 8.0
4. **Restore** - Restores NuGet dependencies
5. **Build** - Compiles the solution in Release mode
6. **Test** - Runs all unit tests
7. **Update nuspec** - Updates version in all nuspec files
8. **Pack** - Creates NuGet packages
9. **Push** - Publishes packages to NuGet.org
10. **Release** - Creates GitHub release (only for tag-based triggers)

### Artifacts

All created packages are uploaded as GitHub Actions artifacts and kept for 30 days.

## Troubleshooting

### "Package already exists" Error

The workflow uses `--skip-duplicate` flag, so it won't fail if the package version already exists.

### Version Mismatch

If you see version mismatches:
1. Check `RapidexAssemblyInfo.cs` has the correct version
2. Ensure the file is committed and pushed
3. Re-run the workflow

### Build Failures

Check the Actions tab for detailed error logs. Common issues:
- Missing dependencies
- Test failures
- Invalid NuGet API key
- Version format issues

### Permission Issues

Ensure the `NUGETORGRAPIDEXDATAAPIKEY` secret has the following permissions:
- Push new packages and package versions
- Update package metadata

## Local Testing

To test package creation locally without publishing:

```bash
# Build solution
dotnet build --configuration Release

# Manually update version in nuspec files or use the version from RapidexAssemblyInfo.cs
VERSION=$(grep -oP 'VERSION = "\K[^"]+' RapidexAssemblyInfo.cs)
echo "Version: $VERSION"

# Create packages
nuget pack package/Rapidex.Data.nuspec -OutputDirectory ./local-packages
nuget pack package/Rapidex.Data.SqlServer.nuspec -OutputDirectory ./local-packages
nuget pack package/Rapidex.Data.PostgreServer.nuspec -OutputDirectory ./local-packages

# Inspect packages
nuget verify -All ./local-packages/Rapidex.Data.*.nupkg
```

## Best Practices

1. **Always update RapidexAssemblyInfo.cs first** before creating a release
2. Use consistent version numbering (semantic versioning)
3. Update CHANGELOG.md with each version
4. Test locally before pushing tags
5. Use meaningful git commit messages
6. Tag format doesn't matter - version always comes from the file

## Security Notes

- Never commit NuGet API keys to the repository
- Use GitHub Secrets for sensitive information
- Regularly rotate your NuGet API keys
- Set appropriate permissions on API keys (package push only)

## GitHub Release

For tag-based publishes, a GitHub release is automatically created with:
- Release notes template
- Links to NuGet packages
- Attached .nupkg files

Edit the release after creation to add detailed release notes.

## Example Workflow

```bash
# 1. Make your code changes
git add .
git commit -m "Add new feature"

# 2. Update version in RapidexAssemblyInfo.cs
# Change VERSION = "0.2.0.0002" to VERSION = "0.2.0.0003"

# 3. Commit version change
git add RapidexAssemblyInfo.cs
git commit -m "Bump version to 0.2.0.0003"

# 4. Push changes
git push

# 5. Create and push tag
git tag v0.2.0.3
git push origin v0.2.0.3

# 6. Watch the GitHub Actions workflow complete
# 7. Packages will be published to NuGet.org automatically

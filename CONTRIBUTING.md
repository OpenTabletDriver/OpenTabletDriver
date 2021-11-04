# Contributing Guidelines

Thank you for showing interest in the development of OpenTabletDriver!

Our goal with this document is to describe how contributions to OpenTabletDriver should be made, depending on what kind of change is being performed.

# Tablet Configuration Contributions

Tablet configurations are the core at what defines a tablet within OpenTabletDriver.
It provides specifications, functions, and initialization data which are all used to make a tablet device functional.

The following rule(s) are applicable to all configuration contributions, whether it be updating a configuration or adding a new one.

- The git branch must be based on and targeting the `configs` branch.

## Adding a Configuration

When adding a new tablet configuration file to the `OpenTabletDriver.Configurations` project, these rules must be followed.

- The `TABLETS.md` file must be updated to include the newly added configuration, including any quirks or missing features.
- The tablet name should follow `[Manufacturer] [Model Number/Product Name]`
- The file name must be the tablet name with the `.json` extension.
- The file must be located in the directory designated to an individual manufacturer.
- All trailing whitespace must be trimmed before committing.
  > When using the configuration editor within the OpenTabletDriver UI, this will be performed automatically.
- The current formatting of the `TABLETS.md` document must be strictly followed as below:

  | Column | Contents                                                                                          |
  | ------ | ------------------------------------------------------------------------------------------------- |
  | Tablet | The name of the tablet, directly from the configuration.                                          |
  | Status | The state of the configuration (Supported, Missing Features, Has Quirks, Broken, Untested)        |
  | Notes  | Any information relating to the status. This can be left empty when marked Supported or Untested. |

  - These support statuses include the following:

    | Status           | Requirements                                                                                                            |
    | ---------------- | ----------------------------------------------------------------------------------------------------------------------- |
    | Supported        | All tablet functions are fully supported without issue.                                                                 |
    | Missing Features | Some tablet functions are unsupported as the driver doesn't support it.                                                 |
    | Has Quirks       | Some tablet functions work, but with undesired behavior or requiring workarounds to get functioning.                    |
    | Broken           | Tested but does not work. This is here for historical purposes, no new configuration will be accepted with this status. |
    | Untested         | Entirely untested, potentially ported from other drivers or from documentation.                                         |

- Include sources to help with verification of the tablet's specifications.
  > This could be the manufacturer's specification sheet of the tablet, the HID report descriptor, or an educated guess by taking MaxX and MaxY and converting it into millimeter units.

## Updating a Configuration

- If the configuration makes a change that changes the support status, it should be updated in `TABLETS.md` according to the same rules as adding the configuration.
- Specify the reasoning for a tablet configuration update in your pull request, otherwise we have no way of knowing why the configuration change is being made.

## Configuration Maintainer Notes

> This applies to any user with push permissions to the `configs` branch.

We use a branch (`configs`) for configuration contributions in order to quickly merge newly added tablet configurations.
This branch will periodically be merged into the `master` branch and the branches will be synchronized.

Ensure that all pull requests modifying or adding configurations have valid specifications and correctly match the status in `TABLETS.md`.

# Code Contributions

We strive to keep a maintainable and easily readable codebase in order to keep OpenTabletDriver alive.

The following rule(s) are applicable to all code contributions:

- The git branch should be based on and targeting the `master` branch (except under certain conditions)
- All trailing whitespace must be trimmed before committing.
- Follow the language's documented formatting guidelines.
- Use spaces for indentation, do not use the tab character.
- Split up individual bug fixes or feature additions into separate pull requests.
  > This makes it easier to pinpoint why changes were made along with the ease of reverting in the case that it is needed.
- If the pull request closes an issue, link that issue with [closing keywords](https://docs.github.com/en/issues/tracking-your-work-with-issues/linking-a-pull-request-to-an-issue#linking-a-pull-request-to-an-issue-using-a-keyword) referring to that issue.

## C# Specific Rules

- Follow the [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions), unless otherwise specified.
- Use 4 spaces for indentation. This can usually be configured in your IDE.
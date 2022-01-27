# Contributing Guidelines

Thank you for showing interest in the development of OpenTabletDriver!

Our goal with this document is to describe how contributions to OpenTabletDriver should be made,
depending on what kind of change is being performed.

# Tablet Configuration Contributions

Tablet configurations are the core at what defines a tablet within OpenTabletDriver. It provides
specifications, functions, and initialization data which are all used to make a drawing tablet
device, also known as the digitizer, functional.

The following rule(s) are applicable to all configuration contributions, whether it be updating a configuration or adding a new one.

- The git branch must be based on and targeting the `configs` branch.

## Adding a Configuration

Documentation the fields within configurations can be found on the official OpenTabletDriver
website, [here](https://opentabletdriver.net/Wiki/Development/Configurations).

When adding a new tablet configuration file to the `OpenTabletDriver.Configurations` project, these
rules must be followed:

- The `TABLETS.md` file must be updated to include the newly added configuration, including any
  quirks or missing features. Please also place the entry near others of it's manufacturer, and in
  alphabetical order from there.
- The tablet name should follow `[Manufacturer] [Model Number/Product Name]`
- The file name must be the tablet name with the `.json` extension.
- The file must be located in the directory designated to an individual manufacturer.
- All trailing whitespace must be trimmed before committing. When using the configuration editor
  within the OpenTabletDriver UI, this will be performed automatically. We recommend using it to
  generate new configs, as it will also format the json correctly with correct indentation, etc.
- The current formatting of the `TABLETS.md` document must be strictly followed as below:

  | Column | Contents                                                                                          |
  | ------ | ------------------------------------------------------------------------------------------------- |
  | Tablet | The name of the tablet, directly from the configuration.                                          |
  | Status | The state of the configuration (one of: Supported, Missing Features, Has Quirks, Broken, Untested) |
  | Notes  | Any information relating to the status. This can be left empty when marked Supported or Untested. |

- A description of each support status:

  | Status           | Requirements                                                                                                              |
  | ---------------- | ------------------------------------------------------------------------------------------------------------------------- |
  | Supported        | All tablet functions are fully supported without issue.                                                                   |
  | Missing Features | Some tablet functions are unsupported as the driver doesn't have support for it (yet).                                    |
  | Has Quirks       | Some tablet functions work, but with undesired behavior or requiring workarounds to get functioning.                      |
  | Broken           | Tested but does not work. This is here for historical purposes, *no new configuration will be accepted with this status.* |
  | Untested         | Entirely untested, potentially ported from other drivers or from documentation. We will potentially accept these, but strongly prefer verification being performed. |

- Include sources to help with verification of the tablet's specifications. This could be the
  manufacturer's specification sheet of the tablet, the HID report descriptor, or an educated guess
  by taking MaxX and MaxY and converting it into millimeter units. Please note that specs published
  by manufacturers have unfortunately been known to be wrong on occasion - avoid trusting them
  without verifying.

## Updating a Configuration

- If the configuration makes a change that *improves* the support status, it should be updated in
  `TABLETS.md` according to the same rules as adding the configuration in the first place.
- Specify the reasoning for a tablet configuration update in your pull request, otherwise we have no
  way of knowing why the configuration change is being made.

## Configuration Maintainer Notes

> This applies to any user with push permissions to the `configs` branch.

We use a branch (`configs`) for configuration contributions in order to quickly merge newly added tablet configurations.
This branch will periodically be merged into the `master` branch and the branches will be synchronized.

Ensure that all pull requests modifying or adding configurations have valid specifications and correctly match the status in `TABLETS.md`.

Changes merged to the `configs` branch should generally only be parsers, json config files, and
similar - anything else that runs the risk of breaking functionality not immediately related to the
tablet in question should be split off, made dependent on the configs PR, and targeted to `master`.
Essentially, merges of `configs` into `master` should ideally not introduce significant breaking behavior or
bugs that would impact other users of other tablets.

# Code Contributions

We strive to keep a maintainable and easily readable codebase in order to keep OpenTabletDriver alive and pleasant to work on.

The following rules apply to all code contributions:

- The git branch should be based on and targeting the `master` branch (except for config
  contributions, mentioned above). Even if a contribution is code, if it's related to configs (eg.
  adding a parser), it should target the configs branch. You should also try to create the branch
  from the most recent commit on master possible, to keep history clean.
- All trailing whitespace must be trimmed before committing. This can be configured in most IDEs to
  happen automatically on file save. If your IDE is capable of recognizing `.editorconfig` files,
  then this will be taken care of for you.
- Follow the [C# Coding
  Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions),
  unless otherwise specified. We don't really do anything particularly eccentric.
- Use spaces for indentation, do not use the tab character. We use 4 space indentation. IDEs that
  respect `.editorconfig` will do this automatically.
- Split up individual bug fixes or feature additions into separate pull requests. If a pull request
  depends on another, you can make the dependency be merged first by putting `- [ ] depends on
  #XXXX` in the description. This makes it easier to pinpoint why changes were made along with being
  faster to review, and easier to revert if necessary.
- Try to avoid opening PRs with many commits - squash commits where reasonable. If you're having
  trouble combining commits, it may be a sign to make multiple PRs instead. However, do not
  over-squash, preserving history and keeping commits readable is important too.
- If the pull request closes an issue, link that issue with [closing
  keywords](https://docs.github.com/en/issues/tracking-your-work-with-issues/linking-a-pull-request-to-an-issue#linking-a-pull-request-to-an-issue-using-a-keyword)
  referring to that issue.

## Setting up your environment

OpenTabletDriver is a fairly simple project to set up for development. As it's a .NET project, you
ideally want an IDE capable of working with C# and .NET projects. VSCode, Rider, Visual Studio, etc
are all suitable.

To get started, create a fork of the repository on GitHub. Then, clone the fork you made, and ensure
you're capable of building OpenTabletDriver, by running either `build.ps1` (for Windows), or
`build.sh` (for Linux). If successful, these scripts will produce executables in a newly created
`bin` subdirectory. You can then run OpenTabletDriver by running the executable for the Daemon and
the UX. For all intents and purposes, this will be a fully functional, though potentially unstable
(as it was compiled from `master`) version of OpenTabletDriver that you can use normally, though do
note there are cases where plugins may not be updated to work on `master`-built versions.

Once you've confirmed you are able to build the application on your computer, you can begin making
changes. Before working, however, ensure you create a new branch to work in - we
recommend/soft-require that pull requests never be made on your fork's `master` branch - this makes
it harder to make further PRs, as you'll have to wait for your original to be merged before you can
make another (as your new PR will contain commits from your previous).

Once you're done, push the created branch to your fork, and open the PR. Fill out the title and
description of what the PR accomplishes, remembering to link any fixed issues. If any part of our
continuous integration fails, fix it - **ideally by amending the most recent commit and force
pushing,** rather than creating commits purely to fix CI, small spelling mistakes, etc.

If asked to make changes or fixes, please make them through GitHub's **batch commit** feature or
locally, and avoid making a commit per fix - making a single "apply fixes from code review" commit
is preferred. If you disagree with a suggested change, provide your reasoning by replying to the
review comment. Resolve applied fixes, if they are not resolved by GitHub automatically.

## Unit-Testing OpenTabletDriver

If you'd like to test OpenTabletDriver locally (our CI runs our unit tests on every PR
automatically), you can run our unit testing through `dotnet` - simply running:

```
dotnet test
```

In the project root will run all tests and report status. The unit tests for the most part concern
the daemon part of the driver, which performs the bulk of the "work" and thus is critical to test.
Of course, we also welcome contributions adding more unit testing, our coverage is weaker outside of
critical areas.

# Contributing Guidelines

Thank you for showing interest in the development of OpenTabletDriver!

Our goal with this document is to describe how contributions to OpenTabletDriver should be made,
depending on what kind of change is being performed.

With any contribution you make, we ask that your commits are named sensibly and similarly to the ones in our [history](https://github.com/OpenTabletDriver/OpenTabletDriver/commits/master).
At the very least, sentence-case your commits, and do consider prefixing the commit message with a component name in similar fashion to `Updater: Add file IncludeList`.
If the reasoning for a commit is potentially cryptic, consider adding a commit description to it, succinctly explaining why it was needed.

# Tablet Configuration Contributions

Tablet configurations define tablets OpenTabletDriver can detect and operate. They provide
specifications, functions, and initialization data which are all used to make a drawing tablet
device, also referred to as a digitizer, functional.

## Adding a Configuration

Documentation for the fields within configurations can be found on the official OpenTabletDriver
website, [here](https://opentabletdriver.net/Wiki/Development/Configurations).

When adding a new tablet configuration file to the `OpenTabletDriver.Configurations` project, these
rules must be followed:

- The `TABLETS.md` file must be updated to include the newly added configuration, including any
  quirks or missing features. Please also place the entry near others of its manufacturer, and in
  alphabetical order from there.
- The tablet name should follow the format `[Manufacturer] [Model Number/Product Name]`
- The file name must be the tablet name, *without the manufacturer
  name,* with a `.json` extension. Spaces are acceptable, and
  preferable to underscores.
- The file must be located in the directory dedicated to an individual manufacturer.
- All trailing whitespace must be trimmed before committing.
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
  | Missing Features | Some tablet functions are unsupported (for now).                                    |
  | Has Quirks       | Some tablet functions work, but with undesired behavior or requiring workarounds to get functioning.                      |
  | Broken           | Tested but does not work. This is here for historical purposes, *no new configuration will be accepted with this status.* |
  | Untested         | Entirely untested, potentially ported from other drivers or from documentation. We will potentially accept these, but strongly prefer verification being performed. |

- Include sources to help with verification of the tablet's specifications. This could be the
  manufacturer's specification sheet of the tablet, the HID report descriptor, or an educated guess
  by taking `MaxX` and `MaxY` and converting it into millimeter units. Please note that specs
  published by manufacturers have unfortunately been known to be wrong on occasion - avoid trusting
  these published specs without verifying the specs are accurate to the physical product.
- Also include a diagnostics file that shows that OTD succesfully detected the tablet.
- If you are the owner of the tablet submitting a new config for your own tablet, you may
  self-verify.

## Updating a Configuration

- If the configuration makes a change that *improves* the support status, it should be updated in
  `TABLETS.md` according to the same rules as adding the configuration in the first place.
- Specify the reasoning for a tablet configuration update in your pull request, otherwise we have no
  way of knowing why the configuration change is being made.


# Code Contributions

We strive to keep a maintainable and easily readable codebase in order to keep OpenTabletDriver
alive and pleasant to work on.

The following rules apply to all code contributions:

- The git branch should be based on and targeting the `master` branch. You should also try to create the branch
  from the most recent commit on master possible, to keep history clean.
- The code you commit must adhere to the rules defined in the project's `.editorconfig` file.
  Some IDEs can recognize `.editorconfig` files and fix some of the broken rules on each file save.
  Otherwise, you can use the `dotnet format OpenTabletDriver` command from the root of the project to sweep through all files.
  To avoid formatting unrelated files, you may want to use the `include` option as documented [here](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-format#options).
  Note that commits on pull requests will go through a formatting check as part of the CI workflow.
- Follow the [C# Coding
  Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions),
  unless otherwise specified. We don't really do anything particularly eccentric, and most potential issues should be covered by our `.editorconfig` setup.
- Use spaces for indentation, do not use the tab character. We use 4 space indentation. IDEs that
  respect `.editorconfig` will do this automatically.
- Split up individual bug fixes or feature additions into separate pull requests. If a pull request
  depends on another, you can make the dependency be merged first by putting `- [ ] depends on
  #<PR_ID>` in the description. This makes it easier to pinpoint why changes were made along with being
  faster to review, and easier to revert if necessary.
- Try to avoid opening PRs with many commits - squash commits where reasonable. If you're having
  trouble combining commits, it may be a sign to make multiple PRs instead. However, do not
  over-squash, preserving history and keeping commits readable is important too.
- If the pull request closes an issue, link that issue with [closing
  keywords](https://docs.github.com/en/issues/tracking-your-work-with-issues/linking-a-pull-request-to-an-issue#linking-a-pull-request-to-an-issue-using-a-keyword)
  referring to that issue.

## Setting up your environment

OpenTabletDriver is a fairly simple project to set up for development. As it's a .NET project, you
ideally want an IDE capable of working with C# and .NET projects, such as:

- [VSCode](https://code.visualstudio.com/) (with C# extensions installed)
- [Rider](https://www.jetbrains.com/rider/)
- [Visual Studio](https://visualstudio.microsoft.com/)

Note that OpenTabletDriver being an open source project means a [yearly free license to use
Rider](https://www.jetbrains.com/community/opensource/#support) can be obtained.

To get started, create a fork of the repository on GitHub. Then, clone the fork you made, and ensure
you're capable of building OpenTabletDriver, by running either `build.ps1` (for Windows), or
`build.sh` (for Linux and MacOS). If successful, these scripts will produce executables in a newly created
`bin` subdirectory. You can then run OpenTabletDriver by running the executable for the Daemon and
the UX. For all intents and purposes, this will be a fully functional, though potentially unstable
(as `master` is a development branch) version of OpenTabletDriver that you can use normally.
Do note that some plugins may not have been updated to work on the latest `master` version.

Once you've confirmed you are able to build the application on your computer, you can begin making
changes. Before working, however, ensure you create a new branch to work in - we
recommend/soft-require that pull requests never be made on your fork's `master` branch - this makes
it harder to make further PRs, as you'll have to wait for your original to be merged before you can
make another (as your new PR will contain commits from your previous).

Once you're done, push the created branch to your fork, and open the PR. Fill out the title and
description of what the PR accomplishes, remembering to link any fixed issues. If any part of our
continuous integration fails, fix it - **ideally by amending/rebasing the most recent commit and force
pushing,** rather than creating commits purely to fix CI, small spelling mistakes, etc.

If asked to make changes or fixes, please make them through GitHub's **batch commit** feature or
locally, and avoid making a commit per fix - making a single "apply fixes from code review" commit
is preferred. If you disagree with a suggested change, provide your reasoning by replying to the
review comment. Resolve applied fixes, if they are not resolved by GitHub automatically.

## Unit-Testing OpenTabletDriver

Our CI runs unit tests on each new commit pushed on a PR, but if you'd like to test OpenTabletDriver locally, you can do so by running the command `dotnet test` from the project root, which will run all tests and report status.
The unit tests for the most part concern the daemon part of the driver, which performs the bulk of the "work" and thus is critical to test.
Of course, we also welcome contributions adding more unit testing, our coverage is weaker outside of
critical areas.

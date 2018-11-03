# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Code Documentation for Exceptions.
- An overload for Set which takes a value without a wraping task. Closes [#30](https://github.com/ChilliCream/greendonut/issues/30).
- Instrumentation API. Closes [#29](https://github.com/ChilliCream/greendonut/issues/29).

### Changed

- Set the _.Net Standard_ version to `1.3` in order to support _.Net 4.6_ framework.

## [1.0.3] - 2018-10-04

### Added

- Changelog file to keep track of changes.
- More tests to improve code coverage.

### Changed

- Switched for most cases to `TaskCreationOptions.RunContinuationsAsynchronously`.
- Improved code documentation for the `DataLoader` class.

## [1.0.2] - 2018-09-27

### Added

- More tests to improve code coverage.

### Removed

- Removed `null` check from `Result.Resolve`, because `null` is a valid value.

## [1.0.1] - 2018-08-30

### Added

- Implemented promise state cleanup.

### Changed

- Solved sonar code smell (_Merged if statements_).

### Removed

- Removed dependency `System.Collections.Immutable`.

## [1.0.0] - 2018-08-30

### Added

- More tests to improve code coverage and to solve concurrency issues.
- Benchmark tests.

### Changed

- Updated build scripts.
- Updated readme.
- Moved to _.net core 2.1_ for test projects.
- Improved _Task_ cache implementation.

### Fixed

- Fixed a few concurrency issues.

## [0.2.0] - 2018-07-30

### Added

- More tests to improve code coverage.
- Default _LRU_ _Task_ cache implementation.
- Code documentation.

### Changed

- Updated readme.

[unreleased]: https://github.com/ChilliCream/greendonut/compare/1.0.3...HEAD
[1.0.3]: https://github.com/ChilliCream/greendonut/compare/1.0.2...1.0.3
[1.0.2]: https://github.com/ChilliCream/greendonut/compare/1.0.1...1.0.2
[1.0.1]: https://github.com/ChilliCream/greendonut/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/ChilliCream/greendonut/compare/0.2.0...1.0.0
[0.2.0]: https://github.com/ChilliCream/greendonut/compare/0.2.0-preview-1...0.2.0

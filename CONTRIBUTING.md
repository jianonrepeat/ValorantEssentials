# Contributing to Valorant Essentials

Thank you for your interest in contributing to Valorant Essentials! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites
- Windows 10 or later
- .NET 8.0 SDK or later
- Git for version control
- Visual Studio 2022 or VS Code (recommended)

### Setting Up Development Environment
1. Fork the repository
2. Clone your fork locally
3. Open the solution in your preferred IDE
4. Build the project to ensure everything works

## Development Guidelines

### Code Style
- Follow C# naming conventions (PascalCase for public members, camelCase for private fields)
- Use meaningful variable and method names
- Keep methods focused and under 50 lines when possible
- Add XML documentation for public methods and classes

### Architecture Guidelines
- Follow the existing service-oriented architecture
- Use interfaces for all services
- Implement proper dependency injection
- Keep UI logic separate from business logic
- Use async/await for I/O operations

### Error Handling
- Always handle exceptions gracefully
- Log errors with appropriate context
- Provide meaningful error messages to users
- Use structured exception handling

### Testing
- Write unit tests for new services
- Test edge cases and error conditions
- Ensure the UI remains responsive during operations
- Test on different Windows versions if possible

## Making Changes

### Before You Start
1. Check existing issues for similar proposals
2. Create a new issue to discuss major changes
3. Ensure your changes align with project goals

### Development Process
1. Create a feature branch from `main`
2. Make your changes following the guidelines
3. Test thoroughly in both Debug and Release modes
4. Update documentation if necessary
5. Submit a pull request with a clear description

### Pull Request Guidelines
- Provide a clear description of changes
- Reference any related issues
- Include screenshots for UI changes
- Ensure all tests pass
- Keep commits focused and atomic

## Areas for Contribution

### Bug Fixes
- Fix crashes or unexpected behavior
- Improve error handling
- Optimize performance issues
- Fix UI responsiveness problems

### Features
- Add new game support
- Implement backup/restore functionality
- Add localization support
- Create plugin architecture

### Documentation
- Improve code comments
- Update README files
- Add usage examples
- Create video tutorials

### Testing
- Add unit tests for services
- Create integration tests
- Test on different Windows versions
- Performance testing

## Reporting Issues

### Bug Reports
Include the following information:
- Windows version
- .NET version
- Steps to reproduce
- Expected behavior
- Actual behavior
- Error messages or logs

### Feature Requests
Provide:
- Clear description of the feature
- Use cases and benefits
- Possible implementation approach
- Any potential drawbacks

## Code Review Process

### What We Look For
- Code quality and readability
- Adherence to architecture guidelines
- Proper error handling
- Performance considerations
- Security implications

### Review Criteria
- Functionality works as intended
- Code follows established patterns
- No breaking changes to existing features
- Documentation is updated
- Tests are included where appropriate

## Release Process

### Version Numbering
We follow semantic versioning (SemVer):
- MAJOR.MINOR.PATCH
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes (backward compatible)

### Release Schedule
- Major releases: As needed for significant changes
- Minor releases: Monthly if there are new features
- Patch releases: As needed for bug fixes

## Community Guidelines

### Code of Conduct
- Be respectful and inclusive
- Welcome newcomers
- Provide constructive feedback
- Focus on the code, not the person
- Help maintain a positive environment

### Communication
- Use clear, professional language
- Be patient with questions
- Provide helpful responses
- Share knowledge and experience

## Questions?

If you have questions about contributing, feel free to:
- Open an issue for discussion
- Check existing documentation
- Review previous pull requests
- Ask in the community forums

Thank you for contributing to Valorant Essentials!
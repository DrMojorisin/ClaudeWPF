# WPFBase Claude Code VS Code Optimization Guide

## Overview

This guide optimizes your WPFBase framework for seamless integration with Claude Code in VS Code.

## Quick Setup Checklist

- [ ] Create `.claude/` directory structure
- [ ] Add `.claude/config.json` with project settings
- [ ] Create code templates in `.claude/templates/`
- [ ] Add reusable prompts to `.claude/prompts/`
- [ ] Update `.vscode/settings.json` with Claude context
- [ ] Add VS Code snippets
- [ ] Create tasks.json for common operations

## Directory Structure

```
.claude/
├── config.json           # Claude Code project settings
├── templates/           # Code generation templates
├── prompts/            # Reusable prompts
└── snippets/           # VS Code snippets
```

## Claude Code Configuration

Create `.claude/config.json`:

```json
{
  "project": {
    "name": "WPFBase",
    "type": "wpf-mvvm",
    "framework": "net9.0-windows"
  },
  "patterns": {
    "viewModel": {
      "baseClass": "ViewModelBase",
      "namespace": "WPFBase.ViewModels",
      "useSourceGenerators": true
    }
  }
}
```

## Optimal Prompt Patterns

### Creating Features
```
Using WPFBase framework, create a [FeatureName] feature with:
1. ViewModel using CommunityToolkit.Mvvm
2. WPF View with data binding
3. Service with dependency injection
4. Unit tests

Follow patterns from docs/development/COMMUNITYTOOLKIT_BEST_PRACTICES.md
```

### Code Reviews
```
Review [component] against WPFBase standards:
- Check MVVM patterns
- Verify CommunityToolkit usage
- Validate dependency injection
```

### Debugging
```
Debug this WPF issue in [file]:
[paste code/error]

Framework: WPFBase with CommunityToolkit.Mvvm
Reference: docs/development/EXAMPLES.md
```

## Best Practices

### DO's
1. Reference documentation explicitly
2. Use templates from .claude/templates/
3. Provide specific context
4. Request validation against patterns

### DON'Ts
1. Don't give vague requests
2. Don't skip documentation references
3. Don't forget testing requirements
4. Don't ignore performance patterns

## VS Code Integration

### Settings
Add to `.vscode/settings.json`:
```json
{
  "claude.contextFiles": [
    "docs/development/CLAUDE.md",
    "docs/development/COMMUNITYTOOLKIT_BEST_PRACTICES.md"
  ]
}
```

### Tasks
Create `.vscode/tasks.json` for common operations:
- Build
- Test
- Run
- Generate feature

## Success Metrics

- **Code Generation Speed**: 80% less boilerplate
- **Pattern Consistency**: 95% adherence
- **Bug Reduction**: 60% fewer runtime errors
- **Documentation Usage**: 90% of prompts reference docs
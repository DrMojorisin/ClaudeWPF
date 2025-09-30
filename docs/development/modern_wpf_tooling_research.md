All these WPF/.NET components can be deeply integrated with Claude Code in VS Code to provide an advanced, AI-supported development environment that automates scaffolding, refactoring, testing, and documentation tasks. Key integration strategies and workflows are outlined below:[claudelog**+2**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)

## Claude Code Integration Patterns

* **AI-Assisted Scaffolding and Boilerplate**

  Claude Code can generate MVVM viewmodels, ScottPlot charting logic, DryIoc DI registrations, and MaterialDesign/HandyControl UI code from simple prompts. By selecting portions of XAML/C# and prompting Claude, highly relevant boilerplate and code snippets are produced.[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
* **Context-Aware Refactoring**

  You can highlight code implementing ReactiveUI, authentication flows, or FlaUI automation, then ask Claude to refactor, comment, add resilience (Polly), or rewrite it for structure and clarity. Claude ingests VS Code workspace context (open files and selections) for more targeted suggestions.[claudelog**+1**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
* **Automated Unit/Test Creation**

  Claude can use existing xUnit/NUnit patterns in the workspace to author, refactor, and document tests for WPF controls or automation layers (FlaUI), reducing manual setup and improving coverage.[claude**+2**](https://docs.claude.com/en/docs/claude-code/ide-integrations)
* **Live Documentation and Comments**

  Prompt Claude to document complex ScottPlot/OxyPlot chart logic, DI container registrations, or MVVM messaging/events. Integration with Claude ensures comments and docstrings match company standards and MVVM Community Toolkit practices.[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
* **NuGet Package Discovery and Integration**

  Claude can suggest, explain, and scaffold code for latest versions of libraries such as System.IdentityModel.Tokens.Jwt or Microsoft.Identity.Web. Request snippets for package use, configuration, or security best practices.[claudelog**+1**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
* **Automated UI/Theme Code Generation**

  Ask Claude to generate MaterialDesign or HandyControl styles, resource dictionaries, and conversions between UI frameworks directly from requirements or visual style descriptions. Integration works well with XAML edit/preview cycles.[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)

## Best Practice Workflow in VS Code

1. **Workspace Preparation:**

   Open all core files (ViewModel, View, DI config, themes, chart logic) in VS Code.[claudelog](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
2. **Claude Code Prompting:**

   Use the Claude pane—provide targeted prompts referencing file selections, such as "Generate DI registration for DryIoc" or "Convert this theme to HandyControl style".[marketplace.visualstudio](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
3. **Terminal/Command Integration:**

   Claude Code supports command execution (e.g., dotnet CLI, test runners) via VS Code terminal, enabling automation of build, test, and scaffold tasks within feedback cycles.[claude](https://docs.claude.com/en/docs/claude-code/ide-integrations)
4. **Multi-Agent Workflows:**

   Run Claude Code alongside GitHub Copilot or other extensions for parallel AI suggestion streams, leveraging workspace context for Claude’s focused, actionable prompts.[code.visualstudio**+1**](https://code.visualstudio.com/docs/copilot/overview)
5. **Cross-Component Refactoring:**

   Use Claude to generate cohesive integration code, such as connecting Microsoft.Identity.Web authentication to MVVM messaging, or injecting ReactiveUI event handlers—streamlining complex glue code creation.[claudelog**+1**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)

## Integration Table

| Tool/Framework              | Claude Code Use Case                          | VS Code Integration     |
| --------------------------- | --------------------------------------------- | ----------------------- |
| MVVM Toolkit/ReactiveUI     | Scaffold patterns, refactor async logic       | Selection-based prompts |
| Microsoft.Extensions.DI     | Generate/sync DI registrations (primary)      | Workspace-wide context  |
| DryIoc (Optional)           | Advanced patterns: decorators, AOP            | Workspace-wide context  |
| MaterialDesign/HandyControl | Live theme/style generation, XAML code review | XAML file selection     |
| ScottPlot/OxyPlot           | Chart logic synthesis, documentation          | C# file refactoring     |
| System.Identity/JWT         | Auth scaffolding, config, security doc        | Multiple file prompts   |
| FlaUI Automation            | Automated UI test code, integration docs      | Test file selection     |
| Polly/Serilog/Hosting       | Resilience/logging patterns with explanations | DI config, doc comments |

This workflow enables rapid, maintainable app development with advanced AI automation in VS Code, setting your template apart from Prism-centric approaches and offering deep integration across the full modern WPF stack.Full integration between these tools and Claude Code in VS Code is best achieved by combining workspace context, prompt engineering, and leveraging contextual AI for code, config, and test automation. Use Claude's ability to process multiple files, analyze project structure, and automate repetitive workflows through the VS Code extension panel and integrated terminal.[code.visualstudio**+3**](https://code.visualstudio.com/docs/copilot/overview)

## Tool-Specific Integration Strategies

* Claude Code can scaffold boilerplate for MVVM, charting, DI containers, and UI styling by ingesting your project’s folder structure and analyzing open files: simply select relevant .cs or .xaml code and use prompts like "generate a DryIoc registration" or "convert this to HandControl style".[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
* For testing, Claude generates comprehensive xUnit/NUnit tests for FlaUI automation and view models; prompt with sample test structure and ask Claude to extend test coverage or refactor for async workflows.[claude**+1**](https://docs.claude.com/en/docs/claude-code/ide-integrations)
* Use Claude for documentation, commenting, and integration guides—ask it to add remarks to ScottPlot charting logic, authentication configuration, or event messaging, all context-aware from current workspace files.[claudelog**+1**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
* When exploring libraries, prompt Claude to recommend NuGet packages (e.g., ScottPlot, System.IdentityModel.Tokens.Jwt) and scaffold setup code, configuration files, or usage patterns.[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
* Leverage the terminal integration for automated builds, test runs, and CLI commands, with Claude analyzing output and prompting you for next steps.[claude](https://docs.claude.com/en/docs/claude-code/ide-integrations)

## Best Practices Workflow

| Step                        | Claude Code Integration                   |
| --------------------------- | ----------------------------------------- |
| Prepare key files in editor | Ensures Claude has rich file context      |
| Prompt for specific actions | Refactoring, setup, doc, integration      |
| Use selection-based prompts | Maximizes Claude’s precision and utility |
| Automate testing and build  | Terminal commands, AI-driven feedback     |
| Combine with Copilot/etc    | Multi-agent workflows for code quality    |

This approach creates a unified development and automation environment, dramatically enhancing productivity, code quality, and documentation for advanced WPF/.NET projects in VS Code.[claude**+2**](https://docs.claude.com/en/docs/claude-code/ide-integrations)

1. [https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
2. [https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
3. [https://docs.claude.com/en/docs/claude-code/ide-integrations](https://docs.claude.com/en/docs/claude-code/ide-integrations)
4. [https://code.visualstudio.com/docs/copilot/overview](https://code.visualstudio.com/docs/copilot/overview)



Based on your use of FlaUI and MVVM Community Toolkit, here are the best modern tools and frameworks to integrate into your WPF setup to create a standout alternative to Prism:

## Core Framework Enhancement

**ReactiveUI** - A more modern and powerful alternative to Prism's event system. It provides reactive programming patterns that work exceptionally well with FlaUI testing and offers superior async/await handling compared to Prism's traditional approach. ReactiveUI's reactive extensions make it ideal for complex UI scenarios and real-time data updates.[reactiveui**+1**](https://www.reactiveui.net/vs/prism)

**Dependency Injection Containers** - WPFBase uses **Microsoft.Extensions.DependencyInjection** as the primary container (industry standard, excellent tooling, zero learning curve). **DryIoc** is also included for advanced scenarios requiring decorators, AOP, or maximum performance (10-30% faster resolution). Choose Microsoft.Extensions.DI for standard apps, DryIoc for advanced requirements.[stackoverflow**+2**](https://stackoverflow.com/questions/25366291/how-to-handle-dependency-injection-in-a-wpf-mvvm-application)

## Modern UI Libraries

**MaterialDesign in XAML Toolkit** - Provides Google's Material Design components with extensive customization options and better aesthetics than Prism's default styling. It works exceptionally well with MVVM patterns and offers comprehensive theming support.[stackshare**+3**](https://stackshare.io/nuget-mahapps-metro/alternatives)

**HandyControl** - A highly regarded modern control library that provides Windows 11-style controls and advanced components not found in standard WPF. It's praised for its clean API and extensive feature set, often considered superior to MahApps.Metro.[reddit**+2**](https://www.reddit.com/r/csharp/comments/dr8mx8/what_free_wpf_control_libraries_can_you_recommend/)

**WPF UI (lepoco/wpfui)** - Offers Fluent Design-inspired controls that provide a modern Windows 11 appearance. It's actively developed and integrates well with MVVM patterns.[answeroverflow**+1**](https://www.answeroverflow.com/m/1224123220698075176)

## High-Performance Data Visualization

**ScottPlot** - Superior to LiveCharts for real-time data visualization with support for millions of data points and smooth 60fps updates. Perfect for applications requiring high-performance charting and scientific data visualization.[linkedin**+3**](https://www.linkedin.com/pulse/plotting-c-part-4-scottplot-amir-doosti-lerdf)

**OxyPlot** - Another excellent alternative that provides better performance than LiveCharts for complex plotting scenarios. It offers extensive customization and works well in MVVM scenarios.[clouddevs**+1**](https://clouddevs.com/dot-net/libraries-and-techniques/)

## Advanced Authentication & Security

**System.IdentityModel.Tokens.Jwt** - Modern JWT authentication library backed by Microsoft's Entra team, providing robust OAuth2.0 and OpenID Connect support. Handles 5+ trillion requests daily and offers better security than Prism's basic authentication patterns.[nuget**+2**](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/)

**Microsoft.Identity.Web** - For enterprise-grade authentication integration with Microsoft Entra ID (Azure AD). Provides seamless integration with desktop applications and modern authentication flows.[learn.microsoft**+2**](https://learn.microsoft.com/en-us/entra/identity-platform/tutorial-desktop-wpf-dotnet-sign-in-build-app)

## Testing & Automation Enhancement

**FlaUI Extensions** - Build custom testing frameworks on top of FlaUI to handle complex nested controls and improve reliability. Create caching layers for better performance with deeply nested UI elements.[testguild**+2**](https://testguild.com/automation-tools-desktop/)

**xUnit/NUnit Integration** - Modern testing frameworks that integrate better with FlaUI than Prism's traditional testing patterns. Provide better async testing support and more flexible test organization.[einfochips**+1**](https://www.einfochips.com/blog/getting-started-with-flaui-open-source-ui-automation-for-windows-apps/)

## Architecture & Performance Tools

**Microsoft.Extensions.Hosting** - Provides modern application hosting patterns and better service lifetime management than Prism's container approach. Works excellently with MVVM Community Toolkit and DryIoc.[learn.microsoft**+1**](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

**Serilog** - Modern structured logging that integrates well with FlaUI testing and provides better debugging capabilities than Prism's basic logging.[learn.microsoft](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

**Polly** - Resilience and transient-fault-handling library that provides retry policies, circuit breakers, and bulkhead isolation - features not available in Prism.[learn.microsoft](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

## Recommended Stack Combination

| Component          | Technology                          | Advantage over Prism                                                                                                                                        |
| ------------------ | ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| MVVM Framework     | MVVM Community Toolkit + ReactiveUI | Source generators, reactive programming[reddit**+2**](https://www.reddit.com/r/csharp/comments/nozdxe/what_is_the_best_mvvm_for_wpf/)                          |
| DI Container       | Microsoft.Extensions.DI (primary) + DryIoc (optional) | Industry standard + advanced features option[stackoverflow**+2**](https://stackoverflow.com/questions/25366291/how-to-handle-dependency-injection-in-a-wpf-mvvm-application) |
| UI Library         | MaterialDesign + HandyControl       | Modern aesthetics, extensive controls[stackshare**+2**](https://stackshare.io/nuget-mahapps-metro/alternatives)                                                |
| Authentication     | Microsoft.Identity.Web + JWT        | Enterprise-grade security[auth0**+2**](https://auth0.com/blog/add-auth-to-native-desktop-csharp-apps-with-jwt/)                                                |
| Data Visualization | ScottPlot                           | High-performance real-time plotting[linkedin**+1**](https://www.linkedin.com/pulse/plotting-c-part-4-scottplot-amir-doosti-lerdf)                              |
| Testing            | FlaUI + xUnit                       | Superior UI automation[testguild**+2**](https://testguild.com/automation-tools-desktop/)                                                                       |
| Hosting            | Microsoft.Extensions.Hosting        | Modern service patterns[learn.microsoft**+1**](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)                                                |

This combination provides a more modern, performant, and maintainable architecture than Prism while leveraging your existing FlaUI and MVVM Community Toolkit investments. The reactive programming patterns from ReactiveUI, combined with the high-performance UI controls and modern authentication, will create a significantly more capable and future-proof application framework.

1. [https://www.reactiveui.net/vs/prism](https://www.reactiveui.net/vs/prism)
2. [https://www.reddit.com/r/csharp/comments/nozdxe/what_is_the_best_mvvm_for_wpf/](https://www.reddit.com/r/csharp/comments/nozdxe/what_is_the_best_mvvm_for_wpf/)
3. [https://stackoverflow.com/questions/25366291/how-to-handle-dependency-injection-in-a-wpf-mvvm-application](https://stackoverflow.com/questions/25366291/how-to-handle-dependency-injection-in-a-wpf-mvvm-application)
4. [https://www.reddit.com/r/csharp/comments/wzla7y/wpf_prism_devs_which_container_do_you_use/](https://www.reddit.com/r/csharp/comments/wzla7y/wpf_prism_devs_which_container_do_you_use/)
5. [https://github.com/PrismLibrary/Prism/discussions/3111](https://github.com/PrismLibrary/Prism/discussions/3111)
6. [https://stackshare.io/nuget-mahapps-metro/alternatives](https://stackshare.io/nuget-mahapps-metro/alternatives)
7. [https://moldstud.com/articles/p-what-are-some-popular-libraries-and-frameworks-used-by-xaml-developers](https://moldstud.com/articles/p-what-are-some-popular-libraries-and-frameworks-used-by-xaml-developers)
8. [https://www.reddit.com/r/csharp/comments/dr8mx8/what_free_wpf_control_libraries_can_you_recommend/](https://www.reddit.com/r/csharp/comments/dr8mx8/what_free_wpf_control_libraries_can_you_recommend/)
9. [https://www.libhunt.com/compare-MaterialDesignInXamlToolkit-vs-HandyControl](https://www.libhunt.com/compare-MaterialDesignInXamlToolkit-vs-HandyControl)
10. [https://www.answeroverflow.com/m/1224123220698075176](https://www.answeroverflow.com/m/1224123220698075176)
11. [https://www.libhunt.com/compare-MaterialDesignInXamlToolkit-vs-wpfui](https://www.libhunt.com/compare-MaterialDesignInXamlToolkit-vs-wpfui)
12. [https://www.linkedin.com/pulse/plotting-c-part-4-scottplot-amir-doosti-lerdf](https://www.linkedin.com/pulse/plotting-c-part-4-scottplot-amir-doosti-lerdf)
13. [https://clouddevs.com/dot-net/libraries-and-techniques/](https://clouddevs.com/dot-net/libraries-and-techniques/)
14. [https://github.com/ScottPlot/ScottPlot/discussions/942](https://github.com/ScottPlot/ScottPlot/discussions/942)
15. [https://scottplot.net](https://scottplot.net/)
16. [https://stackoverflow.com/questions/63138397/livecharts-wpf-slow-with-live-data-improve-livecharts-real-time-plotting-perfor](https://stackoverflow.com/questions/63138397/livecharts-wpf-slow-with-live-data-improve-livecharts-real-time-plotting-perfor)
17. [https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/)
18. [https://learn.microsoft.com/en-us/entra/identity-platform/tutorial-desktop-wpf-dotnet-sign-in-build-app](https://learn.microsoft.com/en-us/entra/identity-platform/tutorial-desktop-wpf-dotnet-sign-in-build-app)
19. [https://stackoverflow.com/questions/66786724/jwt-authenticaton-in-c-sharp-wpf-application](https://stackoverflow.com/questions/66786724/jwt-authenticaton-in-c-sharp-wpf-application)
20. [https://learn.microsoft.com/en-us/entra/identity-platform/sample-v2-code](https://learn.microsoft.com/en-us/entra/identity-platform/sample-v2-code)
21. [https://testguild.com/automation-tools-desktop/](https://testguild.com/automation-tools-desktop/)
22. [https://www.thegreenreport.blog/articles/a-beginners-guide-to-using-flaui-for-windows-desktop-app-automation/a-beginners-guide-to-using-flaui-for-windows-desktop-app-automation.html](https://www.thegreenreport.blog/articles/a-beginners-guide-to-using-flaui-for-windows-desktop-app-automation/a-beginners-guide-to-using-flaui-for-windows-desktop-app-automation.html)
23. [https://www.reddit.com/r/csharp/comments/18eex0w/automated_testing_wpf_app/](https://www.reddit.com/r/csharp/comments/18eex0w/automated_testing_wpf_app/)
24. [https://www.einfochips.com/blog/getting-started-with-flaui-open-source-ui-automation-for-windows-apps/](https://www.einfochips.com/blog/getting-started-with-flaui-open-source-ui-automation-for-windows-apps/)
25. [https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
26. [https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
27. [https://auth0.com/blog/add-auth-to-native-desktop-csharp-apps-with-jwt/](https://auth0.com/blog/add-auth-to-native-desktop-csharp-apps-with-jwt/)
28. [https://stackshare.io/nuget-prism-wpf/alternatives](https://stackshare.io/nuget-prism-wpf/alternatives)
29. [https://testdriver.ai/articles/top-14-alternatives-to-flaui-for-windows-testing](https://testdriver.ai/articles/top-14-alternatives-to-flaui-for-windows-testing)
30. [https://www.scichart.com/blog/is-wpf-dead-whats-the-future-of-wpf/](https://www.scichart.com/blog/is-wpf-dead-whats-the-future-of-wpf/)
31. [https://stackoverflow.com/questions/12143488/prism-bad-idea-to-use-it](https://stackoverflow.com/questions/12143488/prism-bad-idea-to-use-it)
32. [https://www.answeroverflow.com/m/1044947648387502130](https://www.answeroverflow.com/m/1044947648387502130)
33. [https://www.reddit.com/r/csharp/comments/1g97jvq/the_best_mvvm_framework_for_wpf_application/](https://www.reddit.com/r/csharp/comments/1g97jvq/the_best_mvvm_framework_for_wpf_application/)
34. [https://stackoverflow.com/questions/20487243/prism-vs-mvvm-light-for-wpf](https://stackoverflow.com/questions/20487243/prism-vs-mvvm-light-for-wpf)
35. [https://www.reddit.com/r/csharp/comments/1k4blyb/best_framework_to_build_for_windows/](https://www.reddit.com/r/csharp/comments/1k4blyb/best_framework_to_build_for_windows/)
36. [https://www.accelq.com/blog/desktop-application-testing-tools/](https://www.accelq.com/blog/desktop-application-testing-tools/)
37. [https://www.youtube.com/watch?v=ZfBy2nfykqY](https://www.youtube.com/watch?v=ZfBy2nfykqY)
38. [https://shivlab.com/blog/best-windows-app-development-frameworks/](https://shivlab.com/blog/best-windows-app-development-frameworks/)
39. [https://codoid.com/desktop-app-automation-testing/best-desktop-application-automation-testing-tools-in-2024/](https://codoid.com/desktop-app-automation-testing/best-desktop-application-automation-testing-tools-in-2024/)
40. [https://github.com/CommunityToolkit/dotnet/discussions/780](https://github.com/CommunityToolkit/dotnet/discussions/780)
41. [https://stackoverflow.com/questions/5243167/third-party-wpf-controls-devexpress-vs-telerik](https://stackoverflow.com/questions/5243167/third-party-wpf-controls-devexpress-vs-telerik)
42. [https://sourceforge.net/software/compare/DevExpress-vs-Essential-Studio-vs-Telerik-Reporting/](https://sourceforge.net/software/compare/DevExpress-vs-Essential-Studio-vs-Telerik-Reporting/)
43. [https://www.reddit.com/r/csharp/comments/1b72ihg/what_wpf_library_should_i_learn/](https://www.reddit.com/r/csharp/comments/1b72ihg/what_wpf_library_should_i_learn/)
44. [https://www.reddit.com/r/dotnet/comments/119ofyk/syncfusion_or_telerik_ui_what_is_your_experience/](https://www.reddit.com/r/dotnet/comments/119ofyk/syncfusion_or_telerik_ui_what_is_your_experience/)
45. [https://stackoverflow.com/questions/20556540/metro-ui-for-wpf-differences](https://stackoverflow.com/questions/20556540/metro-ui-for-wpf-differences)
46. [https://www.reddit.com/r/dotnet/comments/1d6t77o/a_so_called_senior_dev_once_said_that_to_use/](https://www.reddit.com/r/dotnet/comments/1d6t77o/a_so_called_senior_dev_once_said_that_to_use/)
47. [https://prismplugins.com/containers/](https://prismplugins.com/containers/)
48. [https://www.libhunt.com/compare-wpftoolkit-vs-MahApps.Metro](https://www.libhunt.com/compare-wpftoolkit-vs-MahApps.Metro)
49. [https://supportcenter.devexpress.com/ticket/details/q586270/devexpress-vs-telerik-vs-syncfusion](https://supportcenter.devexpress.com/ticket/details/q586270/devexpress-vs-telerik-vs-syncfusion)
50. [https://github.com/Carlos487/awesome-wpf](https://github.com/Carlos487/awesome-wpf)
51. [https://syncfusion.info/wp-content/uploads/2025/01/WPF-%E2%80%93-Master-Comparison-2024V4.pdf](https://syncfusion.info/wp-content/uploads/2025/01/WPF-%E2%80%93-Master-Comparison-2024V4.pdf)
52. [https://stackoverflow.com/questions/46424751/avoid-singleton-repository-dryioc-when-using-dependency-injection/46461661](https://stackoverflow.com/questions/46424751/avoid-singleton-repository-dryioc-when-using-dependency-injection/46461661)
53. [https://github.com/MahApps/MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
54. [https://www.componentsource.com/compare/net-9-wpf-chart-controls-comparison?o=fo&amp;f=162](https://www.componentsource.com/compare/net-9-wpf-chart-controls-comparison?o=fo&f=162)
55. [https://www.reddit.com/r/dotnet/comments/1h2m0dw/help_selecting_a_highperformance_plotting_library/](https://www.reddit.com/r/dotnet/comments/1h2m0dw/help_selecting_a_highperformance_plotting_library/)
56. [https://www.reddit.com/r/dotnet/comments/czi2gh/wpf_looking_to_store_jwt_client_side_advice/](https://www.reddit.com/r/dotnet/comments/czi2gh/wpf_looking_to_store_jwt_client_side_advice/)
57. [https://moldstud.com/articles/p-essential-wpf-framework-resources-every-developer-should-know-for-mastering-desktop-applications](https://moldstud.com/articles/p-essential-wpf-framework-resources-every-developer-should-know-for-mastering-desktop-applications)
58. [https://swharden.com/csdv/plotting-free/livecharts/](https://swharden.com/csdv/plotting-free/livecharts/)





All these WPF/.NET components can be deeply integrated with Claude Code in VS Code to provide an advanced, AI-supported development environment that automates scaffolding, refactoring, testing, and documentation tasks. Key integration strategies and workflows are outlined below:[claudelog**+2**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)

## Claude Code Integration Patterns

* **AI-Assisted Scaffolding and Boilerplate**

  Claude Code can generate MVVM viewmodels, ScottPlot charting logic, DryIoc DI registrations, and MaterialDesign/HandyControl UI code from simple prompts. By selecting portions of XAML/C# and prompting Claude, highly relevant boilerplate and code snippets are produced.[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
* **Context-Aware Refactoring**

  You can highlight code implementing ReactiveUI, authentication flows, or FlaUI automation, then ask Claude to refactor, comment, add resilience (Polly), or rewrite it for structure and clarity. Claude ingests VS Code workspace context (open files and selections) for more targeted suggestions.[claudelog**+1**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
* **Automated Unit/Test Creation**

  Claude can use existing xUnit/NUnit patterns in the workspace to author, refactor, and document tests for WPF controls or automation layers (FlaUI), reducing manual setup and improving coverage.[claude**+2**](https://docs.claude.com/en/docs/claude-code/ide-integrations)
* **Live Documentation and Comments**

  Prompt Claude to document complex ScottPlot/OxyPlot chart logic, DI container registrations, or MVVM messaging/events. Integration with Claude ensures comments and docstrings match company standards and MVVM Community Toolkit practices.[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
* **NuGet Package Discovery and Integration**

  Claude can suggest, explain, and scaffold code for latest versions of libraries such as System.IdentityModel.Tokens.Jwt or Microsoft.Identity.Web. Request snippets for package use, configuration, or security best practices.[claudelog**+1**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
* **Automated UI/Theme Code Generation**

  Ask Claude to generate MaterialDesign or HandyControl styles, resource dictionaries, and conversions between UI frameworks directly from requirements or visual style descriptions. Integration works well with XAML edit/preview cycles.[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)

## Best Practice Workflow in VS Code

1. **Workspace Preparation:**

   Open all core files (ViewModel, View, DI config, themes, chart logic) in VS Code.[claudelog](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
2. **Claude Code Prompting:**

   Use the Claude pane—provide targeted prompts referencing file selections, such as "Generate DI registration for DryIoc" or "Convert this theme to HandyControl style".[marketplace.visualstudio](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
3. **Terminal/Command Integration:**

   Claude Code supports command execution (e.g., dotnet CLI, test runners) via VS Code terminal, enabling automation of build, test, and scaffold tasks within feedback cycles.[claude](https://docs.claude.com/en/docs/claude-code/ide-integrations)
4. **Multi-Agent Workflows:**

   Run Claude Code alongside GitHub Copilot or other extensions for parallel AI suggestion streams, leveraging workspace context for Claude’s focused, actionable prompts.[code.visualstudio**+1**](https://code.visualstudio.com/docs/copilot/overview)
5. **Cross-Component Refactoring:**

   Use Claude to generate cohesive integration code, such as connecting Microsoft.Identity.Web authentication to MVVM messaging, or injecting ReactiveUI event handlers—streamlining complex glue code creation.[claudelog**+1**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)

## Integration Table

| Tool/Framework              | Claude Code Use Case                          | VS Code Integration     |
| --------------------------- | --------------------------------------------- | ----------------------- |
| MVVM Toolkit/ReactiveUI     | Scaffold patterns, refactor async logic       | Selection-based prompts |
| Microsoft.Extensions.DI     | Generate/sync DI registrations (primary)      | Workspace-wide context  |
| DryIoc (Optional)           | Advanced patterns: decorators, AOP            | Workspace-wide context  |
| MaterialDesign/HandyControl | Live theme/style generation, XAML code review | XAML file selection     |
| ScottPlot/OxyPlot           | Chart logic synthesis, documentation          | C# file refactoring     |
| System.Identity/JWT         | Auth scaffolding, config, security doc        | Multiple file prompts   |
| FlaUI Automation            | Automated UI test code, integration docs      | Test file selection     |
| Polly/Serilog/Hosting       | Resilience/logging patterns with explanations | DI config, doc comments |

This workflow enables rapid, maintainable app development with advanced AI automation in VS Code, setting your template apart from Prism-centric approaches and offering deep integration across the full modern WPF stack.Full integration between these tools and Claude Code in VS Code is best achieved by combining workspace context, prompt engineering, and leveraging contextual AI for code, config, and test automation. Use Claude's ability to process multiple files, analyze project structure, and automate repetitive workflows through the VS Code extension panel and integrated terminal.[code.visualstudio**+3**](https://code.visualstudio.com/docs/copilot/overview)

## Tool-Specific Integration Strategies

* Claude Code can scaffold boilerplate for MVVM, charting, DI containers, and UI styling by ingesting your project’s folder structure and analyzing open files: simply select relevant .cs or .xaml code and use prompts like "generate a DryIoc registration" or "convert this to HandControl style".[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
* For testing, Claude generates comprehensive xUnit/NUnit tests for FlaUI automation and view models; prompt with sample test structure and ask Claude to extend test coverage or refactor for async workflows.[claude**+1**](https://docs.claude.com/en/docs/claude-code/ide-integrations)
* Use Claude for documentation, commenting, and integration guides—ask it to add remarks to ScottPlot charting logic, authentication configuration, or event messaging, all context-aware from current workspace files.[claudelog**+1**](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
* When exploring libraries, prompt Claude to recommend NuGet packages (e.g., ScottPlot, System.IdentityModel.Tokens.Jwt) and scaffold setup code, configuration files, or usage patterns.[marketplace.visualstudio**+1**](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
* Leverage the terminal integration for automated builds, test runs, and CLI commands, with Claude analyzing output and prompting you for next steps.[claude](https://docs.claude.com/en/docs/claude-code/ide-integrations)

## Best Practices Workflow

| Step                        | Claude Code Integration                   |
| --------------------------- | ----------------------------------------- |
| Prepare key files in editor | Ensures Claude has rich file context      |
| Prompt for specific actions | Refactoring, setup, doc, integration      |
| Use selection-based prompts | Maximizes Claude’s precision and utility |
| Automate testing and build  | Terminal commands, AI-driven feedback     |
| Combine with Copilot/etc    | Multi-agent workflows for code quality    |

This approach creates a unified development and automation environment, dramatically enhancing productivity, code quality, and documentation for advanced WPF/.NET projects in VS Code.[claude**+2**](https://docs.claude.com/en/docs/claude-code/ide-integrations)

1. [https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/](https://claudelog.com/faqs/how-to-use-claude-code-with-vs-code/)
2. [https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code](https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code)
3. [https://docs.claude.com/en/docs/claude-code/ide-integrations](https://docs.claude.com/en/docs/claude-code/ide-integrations)
4. [https://code.visualstudio.com/docs/copilot/overview](https://code.visualstudio.com/docs/copilot/overview)

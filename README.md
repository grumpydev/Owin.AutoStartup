Owin.AutoStartup
================

Automatic configuration of AutoStartup compatible OWIN middlewares for compatible hosts.

Getting Started
---------------

Just install the Owin.AutoStartup package, a compatible host (such as Microsoft.Owin.Host.SystemWeb), and a compatible middleware/framework (such as Nancy.Owin), and hit F5.

Troubleshooting
---------------

If diagnostics is enabled, browse to ~/__asdiags to see diagnostic information about loaded startups, and the paths that they're bound to.

Diagnostics is enabled/disabled using the owin:AutoStartupDiagnostics appSetting in the applications app.config/web.config.

Moving From Owin.AutoStartup to a "Normal" OWIN Configuration Class
-------------------------------------------------------------------

For instructions on moving to a normal OWIN configuration class, and for a class definition that will maintain parity with the way your Owin.AutoStartup application is configured, browse to the ~/__asdiags diagnostics page.

Enabling Owin.AutoStartup Support for X framework/middleware
------------------------------------------------------------

Just create a NuGet package, that depends on Owin.AutoStartup, and create a public class that implements IAutoStartup.


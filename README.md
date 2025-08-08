# JiwaAPITests
This is a Visual Studio project using the ServiceStack JsonAPI client on .NET 9 to run automated tests against the Jiwa API.

The config.json file should be edited to provide:
* The Hostname of the API to test against (http://localhost is default)
* The Authentication method (API Key is the default)
* The API Key (Demo data API Key for the user Admin is the default)
* The Credentials to use for credentials authentication (Usename: Admin and Password: password is the default)

If the config.json is not present, one with the default values will be generated when run.

Tests will create their own test data - for instance, when testing Inventory operations the test will create it's own Part with a randomly generated PartNo and use that.
Tests will also remove data created where possible - but note that if a test fails it may not have been able to return the database to it's original state, so a restore or re-create of the database between test runs may be needed.

In Visual Studio
================
Run the tests by loading the solution with Visual Studio 2022 and from the Test menu, choose Run All Tests - the Test Explorer will load and show the tests and their outcomes.

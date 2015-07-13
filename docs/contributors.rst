=================
Contributor Guide
=================

Introduction
============

Contributions to Autofac, whether new features or bug fixes, are deeply appreciated and benefit the whole user community.

The following guidelines help ensure the smooth running of the project, and keep a consistent standard across the codebase. They are guidelines only - should you feel a need to deviate from them it is probably for a good reason - but please adhere to them as closely as possible.

Making Contributions
====================

If you'd like to contribute code or documentation to Autofac, we welcome pull requests and patches. `Questions and suggestions are welcome on the newsgroup. <https://groups.google.com/forum/#!forum/autofac>`_.

Some suggestions for non-code contributions `are provided here <http://kozmic.net/2009/09/06/how-to-contribute-to-open-source-without-writing-a-single-again/>`_.

**Your contributions must be your own work and licensed under the same terms as Autofac.**

Process
=======

Suggest a Feature
-----------------

If you have an idea for an Autofac feature, `the first place to suggest it is on the discussion forum <https://groups.google.com/forum/#!forum/autofac>`_.

Providing code, either via your blog or another distribution outlet, is a great way to get feedback and support from the broader Autofac community. Consider this if it is a possibility, even if it requires core changes. Distributing a modified :code:`Autofac.dll` as a proof-of-concept is encouraged.

If your suggestion applies to a broad range of users and scenarios, it will be considered for inclusion in the core Autofac assemblies. It is likely however that if your suggested feature is experimental, we'll first seek to have it added to the Autofac.Extras features (see below).

Fix a Defect
------------

If you have an issue you'd like fixed, you may also contribute those fixes. **Make sure the issue gets filed** in the `Issue Tracker <https://github.com/autofac/Autofac/issues>`_ so it can be considered.

Git vs. Patches
---------------

Regardless of whether your contribution is accepted for the core or one of the extension assemblies, the preferred means of integrating the code is via `a GitHub pull request <https://help.github.com/articles/using-pull-requests/>`_ rather than a patch. **Submit pull requests to the develop branch instead of master** as we use Gitflow (see below).

If you plan to have ongoing input into the project, ask on `the discussion list <https://groups.google.com/forum/#!forum/autofac>`_ to be added to the committers list. Setting this up will permit issues from the issue tracker to be assigned to you, which is convenient when maintaining code contributions.

Bugs and Code Review Issues
---------------------------

From time to time, issues relating to the work you've done on Autofac may be assigned to you via the issue tracker. Feel free to reassign these to the project owners if you're unable to address the issue.

Announcement
------------

Feel free to announce your changes (once they're built/working/checked in) on the Autofac discussion forum. Include a link to the wiki page and/or a blog post if these apply.

You may also add your name to the list of contributors in the documentation below (this will not be done for you).

License
-------

By contributing to Autofac, you assert that:

1. The contribution is your own original work.
2. You have the right to assign the *copyright* for the work (it is not owned by your employer, or you have been given copyright assignment in writing).
3. You license it under the terms applied to the rest of the Autofac project.

Coding
======

Where to Work / Process
-----------------------

Autofac follows the `Gitflow workflow process <https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow/>`_ for handling releases. This means active development is done on the ``develop`` branch and we push to ``master`` when it's release time. **If you're creating a pull request or contribution, please do it on the develop branch.** We can then build, push to MyGet for testing, and release to NuGet when everything's verified.

Developer Environment
---------------------

There is a **README file in the root of the codeline** (or `read it on GitHub <https://github.com/autofac/Autofac/blob/master/README.md>`_) that explains the expected developer environment and how to build the project.

If your contribution somehow changes the required environment, this document needs to be updated.

Dependencies
------------

The core Autofac assemblies depend on the .NET Base Class Libraries (BCL) only. :code:`Autofac.dll` proper is a Portable Class Library so only depends on a subset of that BCL functionality. This is a conscious decision to keep the project lightweight and easier to maintain.

For core integration assemblies (Autofac.Integration.\*) the latest version of Autofac relies on the latest version of the integration target. For example, Autofac.Integration.Mvc always relies on the latest ASP.NET MVC libraries. This also helps keep the project easier to maintain.

The Autofac.Extras features include assemblies that depend on other Open Source (OSS) libraries. It is important when including new dependencies that:

#. The project can be built straight out of Git (no additional installation needs to take place on the developer's machine). This means NuGet package references and/or checking in dependencies.
#. Any third-party libraries have licenses compatible with Autofac's (the GPL and licenses like it are incompatible - please ask on the discussion forum if you're unsure).

Build Process
-------------

Your contribution will need to be included in the main Autofac solution so it can be included in the build.

If it is a new assembly, you will also need to provide the generation of NuGet packages (library and symbol/source) so the assembly can be published. You should be able to follow the conventions already in the codeline to accomplish this.

**All projects run full FxCop analysis** using a common ruleset. Any new assemblies should also participate in this.

Unit Tests
----------

All contributions to Autofac and Autofac.Extras should be accompanied by unit tests (NUnit) demonstrating the impact of the change. 100% test coverage for code changes is encouraged but not mandatory.

Code Review
-----------

All check-ins to the Autofac source code repository are subject to review by any other project member. Please consider it a compliment that the other developers here will spend time reading your code.

Code review is a great way to share knowledge of how Autofac's internals work, and to weed out possible issues before they get into a binary. If you'd like to contribute to the project by performing code reviews, please jump right in using the code review tools accessible from the commit log.

Documentation
-------------

It is *strongly* encouraged that you update the Autofac wiki when making changes. If your changes impact existing features, the wiki may be updated regardless of whether a binary distribution has been made that includes the changes. A note discussing the version in which behavior changed can be included inline in the wiki, but don't leave obsolete documentation in place - **the documentation on the wiki should remain current so it's not confusing to the reader**.

For new features, consider adding an end-to-end example like on the :doc:`Aggregate Services <../advanced/aggregate-services>` or :doc:`MEF integration <../integration/mef>` pages. This will help users get up to speed and correctly use your feature. There isn't much point contributing code that no one knows how to use :)

**Autofac generates documentation from XML API comments in the code.** Please include these comments when contributing.

**The Golden Rule of Documentation: Write the documentation you'd want to read.** Every developer has seen self explanatory docs and wondered why there wasn't more information. (Parameter: "index." Documentation: "The index.") Please write the documentation you'd want to read if you were a developer first trying to understand how to make use of a feature.

Coding Standards
----------------

Normal .NET coding guidelines apply. See the `Framework Design Guidelines <http://msdn.microsoft.com/en-us/library/ms229042.aspx>`_ for suggestions. If you have access to ReSharper, code should be 'green' - that is, have no ReSharper warnings or errors with the default settings.

Autofac source code uses four spaces for indents (rather than tabs).

The Autofac.Extras Projects
===========================

Autofac.Extras is a companion distribution alongside the main Autofac distribution. The Extras are distinguished by:

- Experimental features
- Integrations with other Open Source projects
- Alternatives to the 'typical' way of doing something in the core (e.g. a different configuration syntax)

In many cases, Autofac.Extras is a way of testing alternatives and getting visibility for new ideas that could eventually end up in the core.

If your contribution is accepted to Autofac.Extras it is unlikely that the rest of the project team will have the knowledge to maintain it, so please expect to have bug reports assigned to you for the area (which you may subsequently reassign if you're unable to action them).

The Wiki / Documentation
========================

If you are doing some renaming or major changes to the wiki, it's easier to check it out and work in a text editor sometimes than it is to do things through the GUI. The location of the wiki source in GitHub is:
https://github.com/autofac/Autofac.wiki.git

Contributors
============

Contributions have been accepted from:

- Nicholas Blumhardt - original version
- Rinat Abdullin - many enhancements
- Petar Andrijasevic - WCF integration
- Daniel Cazzulino - WCF integration enhancements
- Slava Ivanyuk - Moq integration (now part of `Moq Contrib <http://moq-contrib.googlecode.com>`_)
- Craig G. Wilson - additional Resolve() overloads
- C J Berg - perf improvements
- Chad Lee - NHibernate Integration
- Peter Lillevold - generated factories improvements
- Tyson Stolarski - Silverlight port
- Vijay Santhanam - Documentation updates
- Jonathan S. Oliver - resolve bug fix
- Carl Hörberg - various
- Alex Ilyin - bug fixes
- Alex Meyer-Gleaves - scanning improvements
- Mark Crowley - WCF integration improvements
- `Travis Illig <http://www.paraesthesia.com/>`_ - multitenant support
- Steve Hebert - Autofac.Extras.Attributed project for metadata discovery

This isn't a complete list; if you're missing, please add your name or email the project owners.

Mention also has to be made of the many wonderful people who have worked in this field and shared their ideas and insights.

========
Glossary
========

The goal of this page is to help keep documentation, discussions, and APIs consistent.

============== =======
Term           Meaning
============== =======
*Activator*    Part of a *Registration* that, given a *Context* and a set of *Parameters*, can create a *Component Instance* bound to that *Context*
*Argument*     A formal argument to a constructor on a .NET type
*Component*    A body of code that declares the *Services* it provides and the *Dependencies* it consumes
*Instance*     A .NET object obtained by *Activating* a *Component* that provides *Services* within a *Container* (also *Component Instance*)
*Container*    A construct that manages the *Components* that make up an application
*Context*      A bounded region in which a specific set of *Services* is available
*Dependency*   A *Service* required by a *Component*
*Lifetime*     A duration bounded by the *Activation* of an *Instance* and its disposal
*Parameter*    Non-*Service* objects used to configure a *Component*
*Registration* The act of adding and configuring a *Component* for use in a *Container*, and the information associated with this process
*Scope*        The specific *Context* in which *Instances* of a *Component* will be shared by other *Components* that depend on their *Services*
*Service*      A well-defined behavioural contract shared between a providing and a consuming *Component*
============== =======

Admittedly this seems a bit low-level to fit with the typical idea of a 'universal language', but within the domain of IoC containers and specifically Autofac these can be viewed as concepts rather than implementation details.

Wild deviations from these terms in the API or code should be fixed or raised as issues to fix in a future version.

The terms *Application*, *Type*, *Delegate*, *Object*, *Property* etc. have their usual meaning in the context of .NET software development.
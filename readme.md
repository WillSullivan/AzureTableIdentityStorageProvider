##ASP.NET Identity Azure Table StorageProvider

The project is an implementation of `Microsoft.AspNet.Identity.Core` for storing user and role information in an Azure table.

This is still in early development, so don't expect it to work perfectly.  I'll be using this in another project that is undergoing concurrent development, so updates will be forthcoming.

**Please note!** Unit tests depend on you having the *latest version* of the storage emulator installed.  [This is currently in beta.](http://blogs.msdn.com/b/windowsazurestorage/archive/2014/01/16/windows-azure-storage-emulator-2-2-1-preview-release-with-support-for-2013-08-15-version.aspx)

##Version history
(Version histories are the file version, which matches the major/minor for the assembly version)  

* 1.0: Initial checkin
* 1.1: Implementation is generic; user type must extend AzureTableUser.

##Currently missing/failing/FYI
* project documentation on Github

##Latest commit notes
I'm starting to integrate the implementation into my web project.  The first thing I noticed was that having a non-generic implementation was very limiting.

Version 1.1 adds this in.  I tried doing this from the start, but things didn't go very smoothly.  Once the entire thing was written, it was easy to switch to a generic implementation.  There are going to be rough spots with the docs.  I know, I should have spent more time tweaking them.  Assehole move on my part.

I'm also noticing that it is going to be hard to track what has been changed in order to start working on the wiki.  Primarily because  I don't know what the hell I'm doing at this point.  Inspires confidence, no?
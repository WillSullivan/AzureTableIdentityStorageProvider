##ASP.NET Identity Azure Table StorageProvider

The project is an implementation of `Microsoft.AspNet.Identity.Core` for storing user and role information in an Azure table.

This is still in early development, so don't expect it to work perfectly.  I'll be using this in another project that is undergoing concurrent development, so updates will be forthcoming.

**Please note!** Unit tests depend on you having the *latest version* of the storage emulator installed.  [This is currently in beta.](http://blogs.msdn.com/b/windowsazurestorage/archive/2014/01/16/windows-azure-storage-emulator-2-2-1-preview-release-with-support-for-2013-08-15-version.aspx)

##Version history
(Version histories are the file version, which matches the major/minor for the assembly version)  

* 1.0: Initial checkin  
* 1.1: Implementation is generic; user type must extend AzureTableUser.  
* 1.2: First issues with use, first bug fixes..  

##Currently missing/failing/FYI
* Nothing!  You can get the installation instructions [here in the wiki](https://github.com/WillSullivan/AzureTableIdentityStorageProvider/wiki/Installation-and-Setup).

##Latest commit notes
Finally have a chance to start using it. Went boom on first use. ASP.NET Identity apparently trusts the storage provider with the responsibility of assigning Ids to users on creation. Fancy that.  Now creation methods for users and roles will check for a null/empty/whitespace Id and will use a virtual method to assign one if it doesn't exist. Also will throw an ArgumentException on Update or Delete if the Id is not set.

After having to use it, and seeing how chucking everything into the same table results in a gawdawful mess of columns (and possible collisions later on), I've refactored AzureTableStore to require a table name for its methods.  This way each implementation can provide its own table name.

In addition, I've decided to actively map user names to user table partition keys.  This will allow for faster user lookups, but I'm afraid there might be an issue if ASP.NET Identity relies heavily on looking up users by their name.  I'm  watching this for now, so I may revert this in future. The behavior is the same as before by default, using the same partition key.  Inheritors can override a method that is called to map the user name to partition key.  This method is called on every user operation, so manually setting the partition key is pointless.

I've removed the nupkg from the project file, but it's still in source control.  Not sure about that pattern.  

##Previous commit notes
###1.1
I'm starting to integrate the implementation into my web project.  The first thing I noticed was that having a non-generic implementation was very limiting.

Version 1.1 adds this in.  I tried doing this from the start, but things didn't go very smoothly.  Once the entire thing was written, it was easy to switch to a generic implementation.  There are going to be rough spots with the docs.  I know, I should have spent more time tweaking them.  Aishhole move on my part.

I'm also noticing that it is going to be hard to track what has been changed in order to start working on the wiki.  Primarily because  I don't know what the hell I'm doing at this point.  Inspires confidence, no?

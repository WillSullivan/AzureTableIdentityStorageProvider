##ASP.NET Identity Azure Table StorageProvider

The project is an implementation of `Microsoft.AspNet.Identity.Core` for storing user and role information in an Azure table.

This is still in early development, so don't expect it to work perfectly.  I'll be using this in another project that is undergoing concurrent development, so updates will be forthcoming.

**Please note!** Unit tests depend on you having the *latest version* of the storage emulator installed.  [This is currently in beta.](http://blogs.msdn.com/b/windowsazurestorage/archive/2014/01/16/windows-azure-storage-emulator-2-2-1-preview-release-with-support-for-2013-08-15-version.aspx)

##Currently missing/failing/FYI
**This project has NOT YET been proven to work with ASP.NET Identity**
I'm keeping the above line in this section until I can actually get the project to work. 
One step closer.  User and user logins appear to be working correctly now.  Not sure about other parts (e.g., claims).

##Latest commit notes
Quickly learned that Azure Storage queries are *case insensitive*, which can cause a situation where two users can have the same name, but only if they use different casing.  As user names MUST BE distinct in ASP.NET Identity (ugh), this results in the unwanted situation where users can impersonate others easier by using similar names that differ only by case.  If names were not required to be distinct, it would be known that different users can share the same user name, and therefore this wouldn't be an issue.  Unfortunately, it is.

I also got rid of some build warnings.  Yay.

##Version history
(Version histories are the file version, which matches the major/minor for the assembly version)  

* 1.0: Initial checkin  
* 1.1: Implementation is generic; user type must extend AzureTableUser.  
* 1.2: First issues with use, first bug fixes..  
* 1.3: More show-stopping bug fixes
* 1.3.1: Password hashing method assumed the user existed and needed to be updated; fixed
* 1.3.2: User login failed for Google because their provider key sucks at Azure
* 1.3.3: FindByNameAsync now performs case-insensitive searches

##Previous commit notes
###1.1
I'm starting to integrate the implementation into my web project.  The first thing I noticed was that having a non-generic implementation was very limiting.

Version 1.1 adds this in.  I tried doing this from the start, but things didn't go very smoothly.  Once the entire thing was written, it was easy to switch to a generic implementation.  There are going to be rough spots with the docs.  I know, I should have spent more time tweaking them.  Aishhole move on my part.

I'm also noticing that it is going to be hard to track what has been changed in order to start working on the wiki.  Primarily because  I don't know what the hell I'm doing at this point.  Inspires confidence, no?
###1.2
Finally have a chance to start using it. Went boom on first use. ASP.NET Identity apparently trusts the storage provider with the responsibility of assigning Ids to users on creation. Fancy that.  Now creation methods for users and roles will check for a null/empty/whitespace Id and will use a virtual method to assign one if it doesn't exist. Also will throw an ArgumentException on Update or Delete if the Id is not set.

After having to use it, and seeing how chucking everything into the same table results in a gawdawful mess of columns (and possible collisions later on), I've refactored AzureTableStore to require a table name for its methods.  This way each implementation can provide its own table name.

In addition, I've decided to actively map user names to user table partition keys.  This will allow for faster user lookups, but I'm afraid there might be an issue if ASP.NET Identity relies heavily on looking up users by their name.  I'm  watching this for now, so I may revert this in future. The behavior is the same as before by default, using the same partition key.  Inheritors can override a method that is called to map the user name to partition key.  This method is called on every user operation, so manually setting the partition key is pointless.

I've removed the nupkg from the project file, but it's still in source control.  Not sure about that pattern.
###1.3 and 1.3.1
1.3 created because I derped.  The issue I was having was that I was assuming that a user existed and must be updated when applying a password hash.  I've pulled that part out (and deleted some mean tweets).

I had to push an update to NuGet in order to test, and subsequently discovered my error.  Version 1.3 of the NuGet package is therefore worthless, so 1.3.1 is created.
###1.3.2
I finally got my error logging working on my website, so I was able to track down the problem logging in with Google.  The issue turned out to be one where their provider key is a URL, which contains characters not acceptable within a RowKey.  This resulted in a bland 400 error, which required I track down the method whose call caused the exception, examine the data going out, make some guesses on why, prove them with a prototype, and create a one line fix.  Goddamnit.

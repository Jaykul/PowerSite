PowerSite
=========

PowerSite is an attempt to throw together a static page generator in .Net that lets me use the tools and formats I'm used to.

There's a bunch of conventions and configuration that go into a site. The following folders are specially named:


    \posts
        For a blog, blog posts go in this folder
    \pages
        Pages that aren't blog posts go here
    \static
        Static content like images and downloads go here, and will be uploaded to the site without processing.
    \themes
        All the layout templates, javascripts and styles for the themes go here (see below for a discussion of how these work)
    \plugins
        Any plugins used by this site should be installed in this folder

    \output
        The output of rendering the site. It could be destroyed and recreated by a re-render, so don't make changes here.
    \cache
        A cache of partially rendered files with meta information


You may be able to specify additional static folders (and mappings for where to upload them) in the config file, but you cannot change or remove any of these seven folders.


Writing a Post
--------------

Each post starts with an (optional) metadata header composed of key: value pairs. If the first line is three dashes 
(---) then the metadata continues until another line which is just three dashes. Otherwise, the metadata starts at 
the first line of the file and continues until there's a blank line, or any other line that doesn't follow the 
key:value syntax. Note that keys can't have spaces, but values can.  All metadata is passed to any rendering engines 
and templates in your theme (see Themeing, below) for use there.

The default Metadata fields (which you really should provide) are:

    date: yyyy-mm-dd hh:mm
    author: Author Name
    tags: Tag, Another Tag

Name the file with the slug and the markup type. For instance hello-world.md for a markdown post.
If you don't want it published yet, name it with ".draft." in the name, like: hello-world.draft.md



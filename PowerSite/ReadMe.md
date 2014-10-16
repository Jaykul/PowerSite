PowerSite
=========

PowerSite is an attempt to create a static page generator in .Net that lets me use the tools and formats I'm used to.

There's a bunch of conventions and configuration that go into a site. The following folders are specially named:


    \posts
        For a blog, blog posts go in this folder
    \pages
        Pages that aren't blog posts go here (and have a "path" metadata property)
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


Types of Content
----------------

There are three types of content in PowerSite:

==== Static Content

Static content are the files in the \static folder and it's subfolders, which are copied directly to the output, preserving paths. 
You can put anything in here that you like, but the traditional use is to put create folders for images, downloads, etc.


==== Pages

Pages are the files in the `\pages` folder and it's subfolders. 

Like the `\static` content, files in pages are output according to the path that they're in, 
so if you have a `\pages\index.md` it will be the root `\index.html` of your site after rendering, 
and a `\pages\PoshSite\index.md` will output in a "PoshSite" subdirectory.

However, unlike the static files, these are usually markdown files which processed by the renderer, 
and then wrapped in the "page" layout template. If you have HTML files you don't want processed, 
you should put them in `\static`  -- the pages content allows you to create absolutely any structure 
you want for your site in the pages, while still getting the benefit of markdown and layout templates.


==== Posts

Posts are the files in the `\posts` folder.  There's no support for subfolders here.  
These are usually markdown files which are processed by the renderer and then wrapped in the "post" layout template.  
The output location is based on the configuration, and starts with the RootUrl + BlogUrl,
and then a formatted string with support for various tokens:

	${year}		-- the four-digit year
	${month}		-- always zero padded month number
	${day}		-- always zero padded day number
	${time}		-- this is the hh:mm string
	${category}	-- this is the first tag
	${author}	-- the author name
	${title}		-- the post title

Writing a Page or a Post
------------------------

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

Posts also support the *title* field to override the file name (the file name will still be used for the slug, 
but the title will be rendered in the template).

Themes and Layouts
------------------

Each folder in the Themes subfolder is a theme, and the active theme is selected by the "theme" setting in the config.psd1

Subfolders in each theme are output to the site preserving paths, and files which need processing are processed
(currently we only support razor templates, but we should add mustache, as well as non-template langauages like less and sass).

The core template files are the output types: "post" and "page" as well as "index" for lists and "feed" for rss/atom feeds.
The index and feed templates are generated for the whole site, and for each tag and author.

Any additional template files are used only for includes in the core templates.  
Font and image files, as well as css and js are output untouched (preserving paths).

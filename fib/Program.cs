using System.CommandLine;

var bundleOutputOption = new Option<FileInfo>("--output", "File path name");
var bundleLanguageOption = new Option<string>("--language", "language option") { IsRequired = true };
var bundleNoteOption = new Option<bool>("--note", "note option");
var bundleSortOption = new Option<bool>("--sort", "sort option");
var bundleRemoveOption = new Option<bool>("--remove-empty-lines", "remove-empty-lines option");
var bundleAuthorOption = new Option<string>("--author", "author option");

bundleOutputOption.AddAlias("-o");
bundleLanguageOption.AddAlias("-l");
bundleNoteOption.AddAlias("-n");
bundleSortOption.AddAlias("-s");
bundleRemoveOption.AddAlias("-r");
bundleAuthorOption.AddAlias("-a");

var bundleCommand = new Command("bundle", "Bundle code files to single file");
bundleCommand.AddOption(bundleOutputOption);
bundleCommand.AddOption(bundleLanguageOption);
bundleCommand.AddOption(bundleNoteOption);
bundleCommand.AddOption(bundleSortOption);
bundleCommand.AddOption(bundleRemoveOption);
bundleCommand.AddOption(bundleAuthorOption);

string projectPath = AppContext.BaseDirectory;

bundleCommand.SetHandler((output, language, note, sort, remove, author) =>
{
    try
    {
        using (File.Create(output.FullName))
        { }
        using (StreamWriter writer = new StreamWriter(output.FullName))
        {
            if (note)
                writer.WriteLine("//" + projectPath);
            if (author != null)
                writer.WriteLine("//" + author);
            string[] files = new string[0];

            if (language != "all")
            {
                string[] languages = language.Split(',');

                foreach (var lang in languages)
                {
                    string currentLanguage = lang.Trim();
                    if (currentLanguage != "all")
                    {
                        files = files.Concat(Directory.GetFiles(Directory.GetCurrentDirectory(), $"*.{currentLanguage}", SearchOption.AllDirectories)).ToArray();
                    }
                }
            }
            else
            {
                files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories);

            }
            files = files.Where(file => !file.Contains("obj")).ToArray();
            files = files.Where(file => !file.Contains("publish")).ToArray();
            files = files.Where(file => !file.Contains("bin")).ToArray();
            files = files.Where(file => !file.Contains(".vs")).ToArray();
            files = files.Where(file => !file.Contains(".txt")).ToArray();
            files = files.Where(file => !file.Contains(".sln")).ToArray();
            files = files.Where(file => !file.Contains(".rsp")).ToArray();
            if (sort)
                files = files.OrderBy(file => Path.GetExtension(file)).ToArray();
            else
                files = files.OrderBy(file => Path.GetFileName(file)).ToArray();


            foreach (var filePath in files)
            {
                try
                {
                    string[] lines = File.ReadAllLines(filePath);

                    foreach (var line in lines)
                    {
                        if (remove)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                                writer.WriteLine(line);
                        }
                        else
                        {
                            writer.WriteLine(line);
                        }
                    }
                    writer.WriteLine("------------------------------");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error! An error occurred while copying the file");
                }
            }


        }
        Console.WriteLine("The file was created successfully");
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error! The file path is invalid");
    }

}, bundleOutputOption, bundleLanguageOption, bundleNoteOption, bundleSortOption, bundleRemoveOption, bundleAuthorOption);

var rspCommand = new Command("create-rsp", "Create response file");
rspCommand.SetHandler(() =>
{
    using (File.Create("rsp.rsp"))
    { }
    using (StreamWriter writer = new StreamWriter("rsp.rsp"))
    {
        writer.WriteLine("bundle ");
        string output, language, author;
        bool note, sort, remove;
        Console.WriteLine("Enter file-name / path:");
        output = Console.ReadLine();
        if (output != "")
            writer.WriteLine(" --output " + output);
        Console.WriteLine("Enter author name:");
        author = Console.ReadLine();
        if (author != "")
            writer.WriteLine(" --author " + author);
        Console.WriteLine("Insert languages separated by , / all:");
        language = Console.ReadLine();
        if (language != "")
            writer.WriteLine(" --language " + language);
        Console.WriteLine("To add a file path: true / false");
        note = bool.Parse(Console.ReadLine());
        if (note)
            writer.WriteLine(" --note ");
        Console.WriteLine("To sort by file type: true / false");
        sort = bool.Parse(Console.ReadLine());
        if (sort)
            writer.WriteLine(" --sort ");
        Console.WriteLine("To remove empty lines: true / false");
        remove = bool.Parse(Console.ReadLine());
        if (remove)
            writer.WriteLine(" --remove-empty-lines ");
    }
});

var rootCommand = new RootCommand("Root Command for  file builder cli");

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(rspCommand);

rootCommand.InvokeAsync(args);
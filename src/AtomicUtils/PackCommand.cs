﻿using System;
using static System.Console;
using Microsoft.Extensions.CommandLineUtils;
using System.Linq;
using System.IO;

namespace AtomicUtils
{
    class PackCommand : CommandLineApplication
    {
        private readonly CommandOption _offset;
        private readonly CommandOption _target;
        private readonly CommandOption _files;
        private readonly CommandOption _dirs;

        public PackCommand()
        {
            Name = "pack";
            Description = "packs presets into an abu file";
            _offset = Option("-o | --offset", "shifts the preset files this many slots in the target abu file", CommandOptionType.SingleValue);
            _target = Option("-t | --target", "the target abu file path", CommandOptionType.SingleValue);
            _files = Option("-f | --file", "preset file", CommandOptionType.MultipleValue);
            _dirs = Option("-d | --dir", "preset directory", CommandOptionType.MultipleValue);
            HelpOption("-? | -h | --help");
            OnExecute((Func<int>)RunCommand);
        }

        private int RunCommand()
        {
            var target = _target.Value();
            if (string.IsNullOrWhiteSpace(target))
            {
                WriteLine("A target .abu file was not specified");
                return 1;
            }

            var dirFiles = from path in _dirs.Values ?? Enumerable.Empty<string>()
                           let di = new DirectoryInfo(path)
                           from fi in di.EnumerateFiles("*.pre")
                           select fi;

            var files = (from path in _files.Values
                         select new FileInfo(path))
                         .Concat(dirFiles).ToArray();

            if (!files.Any())
            {
                WriteLine("No preset files were specified");
                return 1;
            }

            using (var s = File.OpenWrite(target))
            {
                var offset = 0;

                if (_offset.HasValue())
                    offset = int.Parse(_offset.Value());

                Amplifire.PackagePresets(files, s, offset);
                return 0;

            }
        }
    }
}

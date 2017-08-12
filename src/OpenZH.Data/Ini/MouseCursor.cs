﻿using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class MouseCursor
    {
        internal static MouseCursor Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MouseCursor> FieldParseTable = new IniParseTable<MouseCursor>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseAsciiString() },
            { "Image", (parser, x) => x.Image = parser.ParseAsciiString() },
            { "Directions", (parser, x) => x.Directions = parser.ParseInteger() }
        };

        public string Name { get; private set; }

        public string Texture { get; private set; }
        public string Image { get; private set; }
        public int Directions { get; private set; }
    }
}

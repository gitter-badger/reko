﻿#region License
/* 
 * Copyright (C) 1999-2014 John Källén.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using Decompiler.Core.Output;
using Decompiler.Gui.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;

namespace Decompiler.Gui.Windows
{
    public class TextSpanFormatter : Formatter
    {
        private List<List<TextSpan>> textSpans;
        private List<TextSpan> currentLine;
        private FixedTextSpan currentSpan;
        
        public TextSpanFormatter()
        {
            this.textSpans = new List<List<TextSpan>>();
        }

        public TextViewModel GetModel()
        {
            return new TextSpanModel(textSpans.Select(l => l.ToArray())
                .ToArray());
        }

        public override void Terminate()
        {
            currentSpan = null;
            currentLine = null;
        }

        public override void Write(string s)
        {
            EnsureSpan().Text.Append(s);
        }

        public override void Write(string format, params object[] arguments)
        {
            EnsureSpan().Text.AppendFormat(format, arguments);
        }

        public override void WriteComment(string comment)
        {
            currentSpan = null;
            var span = EnsureSpan();
            span.Style = "cmt";
            span.Text.Append(comment);
            currentSpan = null;
        }
        public override void WriteHyperlink(string text, object href)
        {
            currentSpan = null;
            var span = EnsureSpan();
            span.Style = "link";
            span.Text.Append(text);
            span.Tag = href;
            currentSpan = null;
        }

        public override void WriteKeyword(string keyword)
        {
            currentSpan = null;
            var span = EnsureSpan();
            span.Style = "kw";
            span.Text.Append(keyword);
            currentSpan = null;
        }

        public override void WriteLine()
        {
            currentSpan = null;
            currentLine = null;
        }

        public override void WriteLine(string s)
        {
            EnsureSpan().Text.Append(s);
            currentSpan = null;
            currentLine = null;
        }

        public override void WriteLine(string format, params object[] arguments)
        {
            EnsureSpan().Text.AppendFormat(format, arguments);
            currentSpan = null;
            currentLine = null;
        }

        private FixedTextSpan EnsureSpan()
        {
            if (currentLine == null)
            {
                currentLine = new List<TextSpan>();
                this.textSpans.Add(currentLine);
            }
            if (currentSpan == null)
            {
                currentSpan = new FixedTextSpan();
                this.currentLine.Add(currentSpan);
            }
            return currentSpan;
        }

        private class TextSpanModel : TextViewModel
        {
            private TextSpan[][] lines;

            public TextSpanModel(TextSpan[][] lines)
            {
                this.lines = lines;
            }

            public int LineCount { get { return lines.Length; } }

            public IEnumerable<TextSpan> GetLineSpans(int index)
            {
                return lines[index];
            }
        }

        private class FixedTextSpan : TextSpan
        {
            public StringBuilder Text;

            public FixedTextSpan()
            {
                this.Text = new StringBuilder();
            }

            public override string GetText()
            {
                return Text.ToString();
            }
        }
    }
}
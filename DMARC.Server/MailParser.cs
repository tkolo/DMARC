#region License
// DMARC report aggregator
// Copyright (C) 2018 Tomasz Kołosowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using DMARC.Shared.Model;
using MimeKit;

namespace DMARC.Server
{
    public class MailParser
    {
        public static Report ReadFeedback(Stream stream)
        {
            var xDocument = XDocument.Load(stream);
            var documentRoot = xDocument.Root;
            if (documentRoot is null)
                throw new FormatException();

            return new Report(documentRoot);
        }

        public static IEnumerable<Report> ParseMessage(MimeMessage message)
        {
            foreach (var attachment in message.Attachments.OfType<MimePart>())
            {
                if (string.Equals(attachment.ContentType.MediaSubtype, @"xml", StringComparison.OrdinalIgnoreCase))
                {
                    using (var attachmentStream = attachment.Content.Open())
                    {
                        yield return ReadFeedback(attachmentStream);
                    }
                }
                else if (string.Equals(attachment.ContentType.MediaSubtype, @"gzip", StringComparison.OrdinalIgnoreCase))
                {
                    using (var attachmentStream = attachment.Content.Open())
                    using (var gZipStream = new GZipStream(attachmentStream, CompressionMode.Decompress))
                    {
                        yield return ReadFeedback(gZipStream);
                    }
                }
                else if (string.Equals(attachment.ContentType.MediaSubtype, @"zip", StringComparison.OrdinalIgnoreCase))
                {
                    using (var attachmentStream = attachment.Content.Open())
                    using (var archive = new ZipArchive(attachmentStream, ZipArchiveMode.Read))
                    {
                        foreach (var file in archive.Entries)
                        {
                            using (var entryStream = file.Open())
                            {
                                yield return ReadFeedback(entryStream);
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<Report> ParseStream(Stream mailStream)
        {
            return ParseMessage(MimeMessage.Load(mailStream));
        }
    }
}
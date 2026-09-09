using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;

namespace Core.Helper
{
    public static class DocumentHelper
    {
        public static void FindAndReplaceString(ref Document document, string oldValue, object newValue, bool isHtml = false)
        {
            foreach (Section section in document.Sections)
            {
                foreach (Paragraph paragraph in section.Body.Paragraphs)
                    ReplaceInParagraph(paragraph, oldValue, newValue, isHtml);

                foreach (Paragraph paragraph in section.Paragraphs)
                    ReplaceInParagraph(paragraph, oldValue, newValue, isHtml);

                foreach (var child in section.Tables)
                    if (child is Table table)
                        foreach (TableRow row in table.Rows)
                            foreach (TableCell cell in row.Cells)
                                foreach (Paragraph paragraph in cell.Paragraphs)
                                    ReplaceInParagraph(paragraph, oldValue, newValue, isHtml);

                FindTextBoxesInBody(section.Body, oldValue, newValue, isHtml);
            }
        }

        private static void FindTextBoxesInBody(Body body, string oldValue, object newValue, bool isHtml = false)
        {
            foreach (DocumentObject obj in body.ChildObjects)
            {
                if (obj is Paragraph paragraph)
                    foreach (DocumentObject child in paragraph.ChildObjects)
                    {
                        if (child is TextBox textBox)
                            foreach (Paragraph paragraphChild in textBox.Body.Paragraphs)
                                ReplaceInParagraph(paragraphChild, oldValue, newValue, isHtml);
                        if (child is ShapeObject shape)
                            ReplaceInShape(shape, oldValue, newValue, isHtml);
                        if (child is Table table)
                            foreach (TableRow row in table.Rows)
                                foreach (TableCell cell in row.Cells)
                                    FindTextBoxesInBody(cell, oldValue, newValue, isHtml);
                    }
                else if (obj is Table table)
                    foreach (TableRow row in table.Rows)
                        foreach (TableCell cell in row.Cells)
                            FindTextBoxesInBody(cell, oldValue, newValue, isHtml);
            }
        }

        private static void ReplaceInShape(ShapeObject shape, string oldValue, object newValue, bool isHtml = false)
        {
            foreach (Paragraph paragraph in shape.ChildObjects.OfType<Paragraph>())
                ReplaceInParagraph(paragraph, oldValue, newValue, isHtml);
        }

        private static void ReplaceInParagraph(Paragraph paragraph, string oldValue, object newValue, bool isHtml = false)
        {
            string fullText = paragraph.Text;
            int startIndex = fullText.IndexOf(oldValue);
            if (startIndex == -1) return;

            string newStringValue = string.Empty;
            if (newValue is string)
                newStringValue = newValue.ToString() ?? string.Empty;

            if (isHtml)
            {
                paragraph.Replace(oldValue, "", false, true);
                paragraph.AppendHTML(newStringValue);
            }

            // Nếu không có xuống dòng, dùng Replace đơn giản để giữ format
            if (!newStringValue.Contains("\n"))
            {
                var signs = Enumerable.Range(1, 99).AsEnumerable().Select(x => $"<<Sign{x.ToString("00")}>>");
                if (oldValue.Equals("<<PreparedBySign>>") || signs.Any(x => x.Equals(oldValue)))
                {
                    // Add the image
                    if (newValue is byte[] imageData)
                    {
                        paragraph.Replace(oldValue, "", false, true);

                        DocPicture picture = paragraph.AppendPicture(imageData);
                        // Optional: Adjust image size and position
                        picture.Width = 60;
                        picture.Height = 40;
                        picture.TextWrappingStyle = TextWrappingStyle.InFrontOfText;
                        picture.HorizontalPosition = 100;
                    }
                    else
                    {
                        paragraph.Replace(oldValue, newStringValue, false, true); // Giữ nguyên định dạng
                    }
                }
                else
                    paragraph.Replace(oldValue, newStringValue, false, true); // Giữ nguyên định dạng
                return;
            }

            // Lưu trữ tất cả TextRange và vị trí của chúng
            var textRanges = new List<(TextRange Range, int Start, int Length)>();
            int currentPos = 0;
            foreach (var item in paragraph.Items)
            {
                if (item is TextRange tr)
                {
                    textRanges.Add((tr, currentPos, tr.Text.Length));
                    currentPos += tr.Text.Length;
                }
            }

            // Tìm các TextRange bị ảnh hưởng bởi oldValue
            var affectedRanges = textRanges
                .Where(tr => tr.Start < startIndex + oldValue.Length && tr.Start + tr.Length > startIndex)
                .ToList();

            if (!affectedRanges.Any()) return;

            // Xóa nội dung cũ trong paragraph
            paragraph.Items.Clear();

            // Thêm lại phần trước oldValue
            if (startIndex > 0)
            {
                string beforeText = fullText.Substring(0, startIndex);
                foreach (var tr in textRanges)
                {
                    if (tr.Start < startIndex)
                    {
                        int length = Math.Min(tr.Length, startIndex - tr.Start);
                        TextRange newRange = paragraph.AppendText(beforeText.Substring(tr.Start, length));
                        CopyCharacterFormat(newRange, tr.Range);
                    }
                }
            }

            // Thêm newStringValue với xuống dòng
            string[] lines = newStringValue.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    // Áp dụng định dạng từ TextRange đầu tiên bị thay thế
                    TextRange newRange = paragraph.AppendText(lines[i]);
                    CopyCharacterFormat(newRange, affectedRanges[0].Range);
                }
                if (i < lines.Length - 1)
                {
                    paragraph.AppendBreak(BreakType.LineBreak);
                }
            }

            // Thêm lại phần sau oldValue
            int endIndex = startIndex + oldValue.Length;
            if (endIndex < fullText.Length)
            {
                string afterText = fullText.Substring(endIndex);
                foreach (var tr in textRanges)
                {
                    if (tr.Start + tr.Length > endIndex)
                    {
                        int offset = Math.Max(0, endIndex - tr.Start);
                        int length = tr.Length - offset;
                        if (length > 0)
                        {
                            TextRange newRange = paragraph.AppendText(afterText.Substring(tr.Start + offset - endIndex, length));
                            CopyCharacterFormat(newRange, tr.Range);
                        }
                    }
                }
            }
        }

        private static void CopyCharacterFormat(TextRange target, TextRange source)
        {
            target.CharacterFormat.FontName = source.CharacterFormat.FontName;
            target.CharacterFormat.FontSize = source.CharacterFormat.FontSize;
            target.CharacterFormat.Bold = source.CharacterFormat.Bold;
            target.CharacterFormat.Italic = source.CharacterFormat.Italic;
            target.CharacterFormat.UnderlineStyle = source.CharacterFormat.UnderlineStyle;
            target.CharacterFormat.TextColor = source.CharacterFormat.TextColor;
        }

        public static void MergePdfFiles(string[] inputFiles, string outputFile)
        {
            PdfDocument outputDocument = new PdfDocument();
            foreach (string inputFile in inputFiles)
            {
                PdfDocument inputDocument = PdfReader.Open(inputFile, PdfDocumentOpenMode.Import);
                int count = inputDocument.PageCount;
                for (int i = 0; i < count; i++)
                {
                    PdfPage page = inputDocument.Pages[i];
                    outputDocument.AddPage(page);
                }
            }

            outputDocument.Save(outputFile);
        }
    }
}

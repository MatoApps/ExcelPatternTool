﻿using System.Drawing;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using Workshop.Infrastructure.Services;

namespace Workshop.Infrastructure.Core
{
    internal class XlsStyleBuilder : IStyleBuilder
    {

        private IWorkbook Workbook;
        private short? _palleteColorSize = null;


        public XlsStyleBuilder(IWorkbook workbook)
        {

            Workbook = workbook;
        }

        public short GetBuiltIndDataFormat(string dataFormat)
        {
            var result = HSSFDataFormat.GetBuiltinFormat(dataFormat);
            return result;
        }

        public ICellStyle GetCellStyle(IColor backColor, IColor borderColor, IFont font)
        {
            var cell = Workbook.CreateCellStyle();
            ((HSSFCellStyle)cell).FillForegroundColor = ((HSSFColor)backColor).Indexed;
            ((HSSFCellStyle)cell).LeftBorderColor = ((HSSFColor)borderColor).Indexed;
            ((HSSFCellStyle)cell).RightBorderColor = ((HSSFColor)borderColor).Indexed;
            ((HSSFCellStyle)cell).TopBorderColor = ((HSSFColor)borderColor).Indexed;
            ((HSSFCellStyle)cell).BottomBorderColor = ((HSSFColor)borderColor).Indexed;
            cell.Alignment = HorizontalAlignment.Center;
            cell.BorderBottom = BorderStyle.Thin;
            cell.BorderLeft = BorderStyle.Thin;
            cell.BorderRight = BorderStyle.Thin;
            cell.BorderTop = BorderStyle.Thin;
            cell.FillPattern = FillPattern.SolidForeground;
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.SetFont(font);
            return cell;
        }
        public IColor GetFontColor(ICellStyle cellStyle)
        {
            var result = (cellStyle.GetFont(Workbook) as HSSFFont).GetHSSFColor(Workbook as HSSFWorkbook);
            return result;
        }

        public IFont GetFont(short fontSize, string fontName, IColor fontColor)
        {
            var font = Workbook.CreateFont();
            font.Boldweight = 100;
            ((HSSFFont)font).Color = ((HSSFColor)fontColor).Indexed;
            font.FontName = fontName;
            font.FontHeightInPoints = fontSize;
            return font;
        }

        public IColor GetBackgroundColor(ICellStyle cellStyle)
        {
            var result = (cellStyle as HSSFCellStyle).FillForegroundColorColor;
            return result;

        }
        public IColor GetBoarderColor(ICellStyle cellStyle)
        {
            //can't be realized 
            //var result = (cellStyle as HSSFCellStyle).BottomBorderColor;
            var result = new HSSFColor.Automatic();
            return result;

        }

        public IColor GetColor(string htmlColor)
        {
            if (string.IsNullOrEmpty(htmlColor))
            {
                return new HSSFColor.Automatic();
            }
            Color color = ColorTranslator.FromHtml(htmlColor);
            byte[] array = new byte[]
            {
                color.R,
                color.G,
                color.B
            };
            IColor result;
            HSSFPalette customPalette = (this.Workbook as HSSFWorkbook).GetCustomPalette();
            if (this._palleteColorSize >= 63)
            {
                HSSFColor hSSFColor = customPalette.FindColor(color.R, color.G, color.B);
                if (hSSFColor == null)
                {
                    hSSFColor = customPalette.FindSimilarColor(color.R, color.G, color.B);
                }
                short? palleteColorSize = this._palleteColorSize;
                this._palleteColorSize = (palleteColorSize.HasValue
                    ? new short?((short)(palleteColorSize.GetValueOrDefault() + 1))
                    : null);
                result = hSSFColor;
            }
            else
            {
                if (!this._palleteColorSize.HasValue)
                {
                    this._palleteColorSize = new short?(8);
                }
                else
                {
                    short? palleteColorSize = this._palleteColorSize;
                    this._palleteColorSize = (palleteColorSize.HasValue
                        ? new short?((short)(palleteColorSize.GetValueOrDefault() + 1))
                        : null);
                }
                customPalette.SetColorAtIndex(this._palleteColorSize.Value, color.R, color.G, color.B);
                HSSFColor hSSFColor = customPalette.GetColor(this._palleteColorSize.Value);
                result = hSSFColor;
            }
            return result;
        }
        public IRichTextString GetCommentInfo(string comment)
        {
            var result = new HSSFRichTextString("批注: " + comment);
            return result;
        }

        public IComment GetComment(IRichTextString richTextString)
        {
            //https://www.cnblogs.com/zhuangjolon/p/9300704.html
            HSSFPatriarch patr = (HSSFPatriarch)Workbook.GetSheetAt(0).CreateDrawingPatriarch();
            HSSFComment comment12 = patr.CreateComment(new HSSFClientAnchor(0, 0, 0, 0, 1, 2, 2, 3));//批注显示定位        }
            comment12.String = richTextString;
            comment12.Author = AppConfigurtaionService.Configuration["CellComment:DefaultAuthor"];
            return comment12;

        }

        public IComment GetComment(string comment)
        {
            var text = new HSSFRichTextString("批注: " + comment);
            return GetComment(text);

        }
        public string GetARGBFromIColor(IColor fontColor)
        {
            if (fontColor != null)
            {
                var argb = (fontColor as HSSFColor).GetHexString();
                if (string.IsNullOrEmpty(argb))
                {
                    return null;
                }
                var result = string.Format("#{0}", argb.Substring(2));
                return result;
            }
            return null;
        }

    }
}

using System.Text;
using NUnit.Framework;
using RTLTMPro;

using System;
using UnityEngine;

namespace RTLTMPro.Tests
{
    public class EngageOrderingTests
    {
        // Tests need to replicate what happens in RTLTextMeshPro.GetFixedText()
        //  If you change the code in that method, make the same changes to this function
        protected string SimulatedUpdateText(string text, bool Farsi, bool FixTags, bool PreserveNumbers)
        {
            var output = new FastStringBuilder(RTLSupport.DefaultBufferSize);
            RTLSupport.FixRTL(text, output, Farsi, FixTags, PreserveNumbers);
            output.Reverse();

            return output.ToString();
        }

        [TestCase("فيديو 360؟ انتقل إلى غرفة 360", "ﻓﯿﺪﯾﻮ 063؟ ﺍﻧﺘﻘﻞ ﺇﻟﻰ ﻏﺮﻓﺔ 063")]
        [TestCase("فيديو 360.", "ﻓﯿﺪﯾﻮ 063.")]
        [TestCase("فيديو (360)", "ﻓﯿﺪﯾﻮ )063(")]
        [TestCase("إحضار جميع المستخدمين إلى \"سطح المريخ (المريخ)\"", "ﺇﺣﻀﺎﺭ ﺟﻤﯿﻊ ﺍﻟﻤﺴﺘﺨﺪﻣﯿﻦ ﺇﻟﻰ \"ﺳﻄﺢ ﺍﻟﻤﺮﯾﺦ )ﺍﻟﻤﺮﯾﺦ(\"")]
        [TestCase("\"(المريخ)\"", "\")ﺍﻟﻤﺮﯾﺦ(\"")]
        [TestCase("(\"المريخ\").", ")\"ﺍﻟﻤﺮﯾﺦ\"(.")]
        [TestCase("\"المريخ\".", "\"ﺍﻟﻤﺮﯾﺦ\".")]
        [TestCase("(المريخ).", ")ﺍﻟﻤﺮﯾﺦ(.")]
        [TestCase("هل أنت متأكد أنك تريد حذف \"موسيقىaaa\"؟", "ﻫﻞ ﺃﻧﺖ ﻣﺘﺄﻛﺪ ﺃﻧﻚ ﺗﺮﯾﺪ ﺣﺬﻑ \"ﻣﻮﺳﯿﻘﻰaaa\"؟")]
        [TestCase("هل أنت متأكد أنك تريد حذف \"aaaموسيقى\"؟", "ﻫﻞ ﺃﻧﺖ ﻣﺘﺄﻛﺪ ﺃﻧﻚ ﺗﺮﯾﺪ ﺣﺬﻑ \"aaaﻣﻮﺳﯿﻘﻰ\"؟")]
        [TestCase("المحور المركزي لـ ENGAGE LINK. نحن نعقد جلسات \"تعلم ENGAGE\" هنا بشكل متكرر، بالإضافة إلى حلقات عمل وفعاليات تواصل أخرى. لمزيد من المعلومات، تحقق من لوحات الملاحظات.", "ﺍﻟﻤﺤﻮﺭ ﺍﻟﻤﺮﻛﺰﯼ ﻟـ KNIL EGAGNE. ﻧﺤﻦ ﻧﻌﻘﺪ ﺟﻠﺴﺎﺕ \"ﺗﻌﻠﻢ EGAGNE\" ﻫﻨﺎ ﺑﺸﻜﻞ ﻣﺘﻜﺮﺭ، ﺑﺎﻹﺿﺎﻓﺔ ﺇﻟﻰ ﺣﻠﻘﺎﺕ ﻋﻤﻞ ﻭﻓﻌﺎﻟﯿﺎﺕ ﺗﻮﺍﺻﻞ ﺃﺧﺮﻯ. ﻟﻤﺰﯾﺪ ﻣﻦ ﺍﻟﻤﻌﻠﻮﻣﺎﺕ، ﺗﺤﻘﻖ ﻣﻦ ﻟﻮﺣﺎﺕ ﺍﻟﻤﻼﺣﻈﺎﺕ.")]
        public void CharacterOrder(string input, string expected)
        {
            // Act
            string result = SimulatedUpdateText(input, true, true, true);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}

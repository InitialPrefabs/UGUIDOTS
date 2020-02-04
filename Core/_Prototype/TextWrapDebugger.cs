using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots {

    public class TextWrapDebugger : MonoBehaviour {

        struct LineDiagnostics {
            public float LineWidth;
            public int LineWordCount;
            public int LineCharacterCount;

            public string Chars;

            public override string ToString() {
                return $"LineWidth: {LineWidth}, LineWordCount: {LineWordCount}, LineCharacteCount: {LineCharacterCount}, Chars: {Chars}";
            }
        }

        List<LineDiagnostics> lineInfos;

        void OnDrawGizmos() {
            if (lineInfos == null) {
                lineInfos = new List<LineDiagnostics>();
            }
            lineInfos.Clear();

            var text     = GetComponent<TextMeshProUGUI>();
            var rectSize = text.rectTransform.rect.size;

            var fontScale = text.fontSize / (float)text.font.faceInfo.pointSize;

            var glyphs = text.font.characterLookupTable;

            var diagnostics = new LineDiagnostics {};

            foreach (var c in text.text) {
                glyphs.TryGetValue((uint)c, out var ch);

                var advance = ch.glyph.metrics.horizontalAdvance * fontScale;

                if (c == ' ') {
                    diagnostics.LineWordCount++;
                }

                var width     = diagnostics.LineWidth + advance;
                var charCount = diagnostics.LineCharacterCount + 1;

                if (width < rectSize.x) {
                    diagnostics.Chars              += c;
                    diagnostics.LineWidth           = width;
                    diagnostics.LineCharacterCount  = charCount;
                } else {
                    lineInfos.Add(diagnostics);
                    diagnostics.LineWidth           = advance;
                    diagnostics.LineCharacterCount  = 1;
                }
            }

            foreach (var info in lineInfos) {
                Debug.Log(info.ToString());
            }

            Debug.Log($"Final # of words: {diagnostics.LineWordCount + 1}");
        }
    }
}

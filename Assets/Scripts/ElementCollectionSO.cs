using System;
using UnityEngine;

namespace ChemistryGame
{
    [CreateAssetMenu(menuName = "Chemistry/Elements")]
    public sealed class ElementCollectionSO : ScriptableObject
    {
        [Serializable] public sealed class Element
        {
            public int atomicNumber;
            public string symbol;
            public string georgianName;
            [TextArea] public string fact;
            public int column;
            public int row;
            public Color color;
            public ChemistryQuestionSO discoveryQuestion;
        }
        public Element[] elements;
    }
}

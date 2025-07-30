using UnityEditor;
using UnityEngine;
using utility;

namespace Editor
{
    [CustomEditor(typeof(DebugTools))]
    public class DebugToolsEditor : UnityEditor.Editor
    {
        private ModificationType _selectedModificationType;
        public override void OnInspectorGUI()
        {
            _selectedModificationType = (ModificationType)EditorGUILayout.EnumPopup("Modification Type", _selectedModificationType);

            // Create a button to trigger the BuyModification method
            if (GUILayout.Button("Buy Selected Modification"))
            {
                // Call the method in the target script with the selected enum value
                GameManager.instance.Player.BuyItem(new StoreItem(new Modification(_selectedModificationType), 0));
            }
        }
    }
}
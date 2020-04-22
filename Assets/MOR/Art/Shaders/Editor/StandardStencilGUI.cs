using UnityEngine;

namespace UnityEditor
{
    /// <summary>
    /// Adds to shaders derived from Unity Standard, which is 'internal' so needs the latest manually copied into the project and needs some variables
    /// protection levels changed.
    /// </summary>
    class StandardStencilGUI : StandardShaderGUI
    {
        //MaterialProperty vertexColor = null;
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            
            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (m_FirstTimeApply)
            {
                MaterialChanged(material, m_WorkflowMode);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);

            EditorGUILayout.Space();
            GUILayout.Label("New", EditorStyles.boldLabel);
            
            MakeCheckbox(material,props,"_UseDistanceFade","Use Distance Fade","_USEDISTANCEFADE");
            if ( material.HasProperty("_UseDistanceFade") ) {
                int val =  material.GetInt("_UseDistanceFade") ;
                if ( val == 1 ) {
                    materialEditor.FloatProperty(FindProperty("_FadeDistanceStart",props), "Fade Distance Start");
                    materialEditor.FloatProperty(FindProperty("_FadeDistanceEnd",props), "Fade Distance End");
                }
            }
            
            MakeCheckbox(material,props,"_UseSecondaryColor","Use Secondary Color","_USESECONDARYCOLOR");
            if ( material.HasProperty("_UseSecondaryColor") ) {
                int val =  material.GetInt("_UseSecondaryColor") ;
                if ( val == 1 ) {
                    materialEditor.ColorProperty(FindProperty("_SecondaryColor",props), "SecondaryColor");
                    materialEditor.ColorProperty(FindProperty("_SecondaryEmission",props), "SecondaryEmissionColor");
                }
            }
            EditorGUILayout.Space();
            bool customBlendProperties = (BlendMode) material.GetFloat("_Mode") == BlendMode.Custom;
            if(customBlendProperties) {
                materialEditor.RenderQueueField();
            }

            int startOfNew = FindPropertyIndex(props,"_Mode");
            if (customBlendProperties == false) {
                startOfNew = FindPropertyIndex(props,"_ZWrite");
            }

            PropertiesDefaultGUI(props,startOfNew);
        }

        protected int FindPropertyIndex(MaterialProperty[] properties, string propertyName) {

            for (int index = 1; index < properties.Length; index++)
            {
                if (properties[index] != null && properties[index].name == propertyName) {
                    return index;
                }
            }

            return -1;
        }
        /// <summary>
        ///   <para>Default rendering of shader properties.</para>
        /// </summary>
        /// <param name="props">Array of material properties.</param>
        public void PropertiesDefaultGUI(MaterialProperty[] props,int startIndex)
        {
            m_MaterialEditor.SetDefaultGUIWidths();
            for (int index = startIndex+1; index < props.Length; index++){
                if ((props[index].flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == MaterialProperty.PropFlags.None) {
                    m_MaterialEditor.ShaderProperty(EditorGUILayout.GetControlRect(true, m_MaterialEditor.GetPropertyHeight(props[index], props[index].displayName), EditorStyles.layerMaskField), props[index], props[index].displayName);
                }
            }
        }
        protected void MakeCheckbox(Material material,MaterialProperty[] props, string materialProperty,string label, string keyword) {
            if ( !material.HasProperty(materialProperty) ) {
                return;
            }

            int val =  material.GetInt(materialProperty) ;
            MaterialProperty useVertexColor = FindProperty(materialProperty, props);
            useVertexColor.floatValue = GUILayout.Toggle(val == 1, label) ? 1f:0f;
            if ( string.IsNullOrWhiteSpace(keyword) != false ) {
                return;
            }

            if ( val == 1 ) {
                material.EnableKeyword(keyword);
            } else {
                material.DisableKeyword(keyword);
            }
        }

    }
} // namespace UnityEditor

using EasyButtons;
using UnityEngine;
[ExecuteInEditMode]
public class MORPoster : MonoBehaviour {
	public Texture2D posterTexture;
	private MaterialPropertyBlock propertyBlock;
	private MeshRenderer meshRenderer;
	private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
	private static readonly int MainTex = Shader.PropertyToID("_MainTex");

	
	private void Awake(){
		propertyBlock = new MaterialPropertyBlock();
		meshRenderer = GetComponent<MeshRenderer>();
		SetPoster();
	}

	[Button]
	private void ScalePoster(){
		if (posterTexture == null) {
			return;
		}
		var dimension =(float)posterTexture.width/ posterTexture.height;
		Vector3 localScale = transform.localScale;
		localScale.x = -localScale.y * dimension;//we want negative so poster glows on wall and backface shows properly
		transform.localScale = localScale;
	}
	[Button]
	private void SetPoster(){
		if (posterTexture) {
			return;
		}
		meshRenderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetTexture(MainTex, posterTexture);
		propertyBlock.SetTexture(EmissionMap, posterTexture); //Set emission Tecture to same as main texture;
		meshRenderer.SetPropertyBlock(propertyBlock);
	}
}

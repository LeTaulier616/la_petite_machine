using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AmplifyColorVolume))]
public class ColorZone : MonoBehaviour
{
	[SerializeField]
	public ColorController.ColorChoice zoneColor;

	private AmplifyColorEffect cameraEffect;
	public AmplifyColorVolume volume;

	[SerializeField]
	private Color wireColor;

	// Use this for initialization
	void Start () 
	{
		cameraEffect = Camera.main.GetComponent<AmplifyColorEffect>();
		volume = GetComponent<AmplifyColorVolume>();

		volume.EnterBlendTime = cameraEffect.ExitVolumeBlendTime;

		if(renderer != null)
			renderer.enabled = false;

		UpdateZoneVariables(zoneColor, volume);
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void UpdateZoneVariables(ColorController.ColorChoice choice, AmplifyColorVolume volume)
	{
		if(choice == ColorController.ColorChoice.Left)
			volume.LutTexture = ColorController.Instance.colorParameters.leftTexture;

		else if (choice == ColorController.ColorChoice.Right)
			volume.LutTexture = ColorController.Instance.colorParameters.rightTexture;

		else
			Debug.LogWarning("Zone " + gameObject.name + " is set on None. Invalid setting");
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			StopCoroutine(ExitVolume());
			StartCoroutine(EnterVolume());
		}
	}

	void OnTriggerExit(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			StopCoroutine(EnterVolume());
			StartCoroutine(ExitVolume());
		}
	}

	void OnDrawGizmos()
	{
		BoxCollider bc = GetComponent<BoxCollider>();

		if(zoneColor == ColorController.ColorChoice.Left)
			wireColor = Color.red;
			//wireColor = ColorController.Instance.colorParameters.leftColor;

		else if(zoneColor == ColorController.ColorChoice.Right)
			wireColor = Color.green;
			//wireColor = ColorController.Instance.colorParameters.rightColor;

		if (bc != null)
		{
			Gizmos.color = wireColor;
			Gizmos.DrawIcon( bc.bounds.center, "lut-volume.png", true );
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube( bc.center, bc.size );
		}
	}

	void OnDrawGizmosSelected()
	{
		BoxCollider bc = GetComponent<BoxCollider>();

		if(zoneColor == ColorController.ColorChoice.Left)
			wireColor = Color.red;
			//wireColor = ColorController.Instance.colorParameters.leftColor;
		
		else if(zoneColor == ColorController.ColorChoice.Right)
			wireColor = Color.green;
			//wireColor = ColorController.Instance.colorParameters.rightColor;

		if ( bc != null )
		{
			Color col = wireColor;
			col.a = 0.2f;
			Gizmos.color = col;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube( bc.center, bc.size );
		}
	}

	IEnumerator EnterVolume()
	{
		ColorController.Instance.CanChangeColor(false);
		ColorController.Instance.currentColor = zoneColor;
		
		yield return new WaitForSeconds(volume.EnterBlendTime);
		
		ColorController.Instance.switchColor(ColorController.Instance.currentColor);
	}

	IEnumerator ExitVolume()
	{
		ColorController.Instance.currentColor = ColorController.ColorChoice.None;

		yield return new WaitForSeconds(volume.EnterBlendTime);

		ColorController.Instance.switchColor(ColorController.Instance.currentColor);
		ColorController.Instance.CanChangeColor(true);
	}
}

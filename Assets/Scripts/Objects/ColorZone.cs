using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AmplifyColorVolume))]
public class ColorZone : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	public ColorController.ColorChoice zoneColor;

	private AmplifyColorEffect cameraEffect;

	[HideInInspector]
	public AmplifyColorVolume volume;

	[HideInInspector]
	[SerializeField]
	private Color wireColor;

	public ColorCannon[] linkedCannons;

	// Use this for initialization
	void Start () 
	{
		cameraEffect = Camera.main.GetComponent<AmplifyColorEffect>();
		volume = GetComponent<AmplifyColorVolume>();

		volume.EnterBlendTime = cameraEffect.ExitVolumeBlendTime;

		if(renderer != null)
			renderer.enabled = false;

		UpdateZoneVariables(zoneColor, volume);

		SetLinkedCannons(linkedCannons);
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

	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			StopCoroutine(ExitVolume());
			StartCoroutine(EnterVolume());
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			StopCoroutine(EnterVolume());
			StartCoroutine(ExitVolume());
		}
	}

	private void OnDrawGizmos()
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

	private void OnDrawGizmosSelected()
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

	private void SetLinkedCannons(ColorCannon[] linkedCannons)
	{
		foreach(ColorCannon canon in linkedCannons)
		{
			canon.SetColor(zoneColor);
		}
	}

	IEnumerator EnterVolume()
	{
		ColorController.Instance.CanChangeColor(false);
		PlayerController.Instance.InColorZone(true);
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
		PlayerController.Instance.CanShoot(false);
		PlayerController.Instance.InColorZone(false);
		PlayerController.Instance.SetCannonColor(ColorController.ColorChoice.None);
	}
}

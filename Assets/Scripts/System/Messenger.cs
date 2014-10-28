/*
 * /*$* <Synapse: LVO> - <Explore the museum and travel through time and space to learn the Low Visibility Operation course>
 *
 * Copyright (c) <2014> AIRBUS Operations.
 *
 * Contributors:
 *  <Buratto Florence> 			[(<Airbus France>)] [<Florence.buratto@airbus.com>]
 *  <Beaudroit Amelie> 			[(<U-Topiq La maison de l’initiative>)] [<amelie.beaudroit@gmail.com>]
 *  <Cabrera Louis> 			[(<Airbus France>)] [<louis.arthur.cabrera@gmail.com>]
 *  <Bernardoff Charles> 		[(<Airbus France>)] [<charles.bernardoff@gmail.com>]
 *  <Gosselin Pauline> 			[(<Airbus France>)] [<pauline.gosselin75@gmail.com>]

 * All rights reserved.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class Messenger {
	[HideInInspector]
	public string name = "Notify";
	
	public GameObject target;
	public string methodName;
	public float delay = 0;
	public bool ignoreTimeScale = false;
	public bool forceActive;


	public void Send()
	{
		if(target != null) {
			if(!target.activeSelf)
			{
				if(forceActive)
					target.SetActive(true);
			}
			target.SendMessage(methodName);
		}
	}


}

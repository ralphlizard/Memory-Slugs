using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Human : NetworkBehaviour {

	public bool havePoked;
	public float turnSpeed;
	int targetIndex = 0;
	int targetsSize = 0;
	float minDistance = 0;
	bool waiting;
	Animator anim;
	NavMeshAgent agent;
	public Transform[] targets;
	Vector3 startPos;
	Quaternion startRot;
	public float angleThreshold = 5;

	[SyncVar]
	Transform curTarget;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this);
		startPos = transform.position;
		startRot = transform.rotation;
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
		minDistance = agent.stoppingDistance;
		Initialize();
	}

	public void Initialize()
	{
		targets = new Transform[13];
		anim.SetTrigger ("reset");
		Stop();
		transform.position = startPos;
		transform.rotation = startRot;
		targetsSize = 0;
		targetIndex = 0;
		waiting = false;
		havePoked = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.S))
		{
			Stop();
		}
		if (waiting)
		{
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.sit"))
			{
				anim.SetTrigger("idle");
				waiting = false;
			}
			if (agent.remainingDistance > minDistance &&
				anim.GetCurrentAnimatorStateInfo(0).IsName("human.idleStand")) //target is far away
			{
				Walk();
				waiting = false;
			}
			if (agent.remainingDistance <= minDistance) //near enough to touch balloon and facing it
			{
				Stop();
				Poke();
				targetIndex++;
				waiting = false;
			}
		}

		if (havePoked &&
			anim.GetCurrentAnimatorStateInfo(0).IsName("human.pokeRecoil"))
		{
			//done poking
			curTarget.SendMessageUpwards("ReceivePoke");
			havePoked = false;
		}

		if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.sit") ||
			anim.GetCurrentAnimatorStateInfo(0).IsName("human.idleStand"))
			//standing around. find a target
		{
			if (targets[targetIndex] != null) //has a target queued
			{
				curTarget = targets[targetIndex];
				agent.destination = curTarget.GetComponentInChildren<BalloonPop>().transform.position;
				waiting = true;
			}
		}

		/*
		if (arrived) //near enough to touch but looking wrong way
		{
			Vector3 targetDir = agent.destination - transform.position;
			targetDir = new Vector3 (targetDir.x, 0, targetDir.z);
			Vector3 newFor = new Vector3 (transform.forward.x, 0, transform.forward.z);
			float angleDiff = Vector3.Angle(targetDir, newFor);
			if (angleDiff > angleThreshold) //looking wrong way
			{
				TurnToTarget();
			}
			else
			{
				waiting = true;
			}
		}*/
	}

	void Stop()
	{
		agent.Stop();
		anim.SetBool("isWalking", false);
		anim.SetTrigger("idle");
	}

	void Walk()
	{
		print("walk");
		agent.Resume();
		anim.SetBool("isWalking", true);
	}

	void Poke()
	{
		anim.SetTrigger("poke");
		havePoked = true;
	}

	public void AddTarget (Transform targetTransform)
	{
		targets[targetsSize] = targetTransform;
		targetsSize++;
	}
}
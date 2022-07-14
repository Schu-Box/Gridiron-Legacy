using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attribute
{
	public AttributeGroup attributeGroup;
	public AttributeType type;
	public float value; // How good they are at it

	private float startingFloor = 0f;
	private float startingCap = 0.6f;

	public Attribute(AttributeType at, AttributeGroup ag)
	{
		attributeGroup = ag;
		type = at;
		value = startingFloor + Random.value * startingCap;
	}

	public Attribute(AttributeType at, AttributeGroup ag, float startingValue)
	{
		attributeGroup = ag;
		type = at;
		value = startingValue;
	}
}

public enum AttributeType 
{
	//Rushing
	RushingElusiveness,
	RushingStrength,
	RushingHustle,
	RushingSpeed,
	BallSecurity,

	RushingDurability,
	RushingHarmfulness,

	//Passing
	PassingAccuracy,
	PassingStrength,
	PassingElusiveness,
	PassingHustle,

	PassTendency,
	PassingDurability,

	//Receiving
	RouteRunning,
	ReceivingStrength,
	ReceivingSpeed,
	ReceivingHands,
	ReceivingBoxoutability,

	FumbleRecovery,
	ReceivingDurability,

	//Blocking
	PassBlocking,
	RushBlocking,
	RushAssistance, //Unused rn

	BlockingDurability,
	BlockingHarmfulness,

	//Tackling
	TacklingPursuit,
	TacklingStrength,
	TacklingHustle,
	PeanutPunchability,

	TacklingDurability,
	TacklingHarmfulness,

	//Blitzing
	BlitzContain,
	BlitzPenetration, 
	BlitzPursuit,
	BlitzHustle,

	BlitzDurability,
	BlitzHarmfulness,

	//Coverage
	CoverageJamming,
	CoverageBlanketness,
	CoverageBreakupability,
	CoverageInterceptablity,
	RecoverySpeed,

	CoverageDurability,
	CoverageHarmfulness,

	//Kicking
	PuntingStrength,
	PuntingAccuracy,
	PuntingPrecision,

	KickingStrength,
	KickingAccuracy,

	KickingDurability,

	//Overall (Other):
	RecoveryRate
}


public enum AttributeGroup
{
	Rushing, 
	Blocking,
	Receiving,
	Passing,

	Tackling,
	Blitzing,
	Coverage,

	Kicking
}

////tendencies
//public float passTendency; //How likely they are to pass (1) or run (0)
//						   //side preference?
//						   //float blitzTendency; //NOTE: Depends on how I handle blitzers being able to tackle rushers

//public float fumbleRecovery; //How likely they are to recover a fumble

////running stats
//public float rushingElusiveness; //How likely they are to dodge a tackle attempt
//public float rushingStrength; //How likely they are to break a tackle
//public float rushingHustle; //How likely they are to gain or lose yards on a tackle
//public float rushingSpeed; //??? How likely that another defender has a tackle opportunity
//public float ballSecurity; //How likely they are to avoid a fumble

////passing stats
//public float passingAccuracy; //How likely they are to throw a catchable ball
//public float passingStrength; //How far they can possibly throw the ball
//public float passingElusiveness; //How likely they are to evade a blitzer - measured vs blitzPursuit
//public float passingHustle; //How likely they are to lose yards on a sack - measured vs blitzHustle

////receiving stats
//public float routeRunning; //How likely they are to have separation on defender (extra modifier)
//public float receivingSpeed; //Determines how many yards downfield the catch attempt occurs - measured w/ passingStrength vs protectionJamming
//public float receivingHands; //How likely they are to catch a thrown ball - measured w/ passingAccuracy vs protectionBreakupability

////blocking stats
//public float passBlocking; //How likely they are to block a defender each pass - measured vs blitzStrength;
//public float rushBlocking; //How likely they are to block a defender from initial rush tackle - measured vs blitzContain
//public float rushAssistance; //How likely they are to increase the distance a rusher will run

////tackling stats
//public float tacklingPursuit; //How likely they are to successfully attempt a tackle measured vs rushingElusiveness
//public float tacklingStrength; //How likely they are to make a tackle
//public float tacklingHustle; //How likley they are to give up yards or tackle for loss
//public float peanutPunchability; //How likely they are to force a fumble

////blitzing stats
//public float blitzContain; //How likely they are to beat a Guard attempting to prevent their tackle attempt - measured vs rushBlocking
//public float blitzPenetration; //How likely they are to beat their blocker - measured vs passBlocking
//public float blitzPursuit; //How likely they are to make a tackle attempt on a passing back - measured vs passingElusiveness
//public float blitzHustle; //How likely they are to sack for a loss of yards - measured vs passingHustle


////protection stats
//public float protectionJamming; //How likely they are to reduce the amount of yards per catch attempt - measured vs passingStrength and receivingSpeed
//public float protectionCoverage; //How likely they are to stay with a fast receiver - measured vs routeRunning
//public float protectionBreakupability; //How likely they are to prevent a catch if they manage to stay with a receiver
//public float protectionInterceptability; //How likely they are to have an interception opportunity
//public float protectionSpeed; //Influences how many yards are gained during AdditionalTackleAttempt - measured vs rushingSpeed

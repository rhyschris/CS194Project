using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameState {
	//Masks for flags
	public const byte p1AttackMask = 0x01; //1st bit
	public const byte p1BlockMask = 0x02; //2nd bit
	public const byte p1CrouchMask = 0x04; //3rd bit
	public const byte p1HighMask = 0x08; //4th bit

	public const byte p2AttackMask = 0x10; //5th bit
	public const byte p2BlockMask = 0x20; //6th bit
	public const byte p2CrouchMask = 0x40; //7th bit
	public const byte p2HighMask = 0x80; //8th bit


	//Position
	private float player1X;
	private float player1Y;
	private float player2X;
	private float player2Y;

	//Health
	private float player1Health;
	private float player2Health;

	//Each players status: isAttacking, isCrouching, isBlocking, high/low (block and attack mutually exlcusive)
	private byte actionFlags;


	public GameState(float p1PosX, float p1PosY, float p2PosX, float p2PosY, float p1health, float p2health){
		player1X = p1PosX;
		player1Y = p1PosY;
		player1Health = p1health;

		player2X = p2PosX;
		player2Y = p2PosY;
		player2Health = p2health;

		actionFlags = 0;
	}

	public void setFlags(bool p1Attacking,bool p1Blocking,bool p1Crouching,bool p1High,
		bool p2Attacking,bool p2Blocking,bool p2Crouching,bool p2High)
	{

		actionFlags |= p1Attacking ? p1AttackMask : (byte)0;
		actionFlags |= p1Blocking ? p1BlockMask : (byte)0;
		actionFlags |= p1Crouching ? p1CrouchMask : (byte)0;
		actionFlags |= p1High ? p1HighMask : (byte)0;

		actionFlags |= p2Attacking ? p2AttackMask : (byte)0;
		actionFlags |= p2Blocking ? p2BlockMask : (byte)0;
		actionFlags |= p2Crouching ? p2CrouchMask : (byte)0;
		actionFlags |= p2High ? p2HighMask : (byte)0;

	}
	public List<float> getFloatList(){
		List<float> myFloats = new List<float>();
		myFloats.Add(player1X);
		myFloats.Add(player1Y);
		myFloats.Add(player1Health);
		myFloats.Add(player2X);
		myFloats.Add(player2Y);
		myFloats.Add(player2Health);

		return myFloats;
	}
	public byte getFlags(){
		return actionFlags;
	}

	public float getP1XPos(){
		return player1X;
	}
	public float getP2XPos(){
		return player2X;
	}
}

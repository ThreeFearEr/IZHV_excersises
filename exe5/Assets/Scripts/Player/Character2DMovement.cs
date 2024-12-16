using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Simple 2D character movement processor.
/// </summary>
public class Character2DMovement : MonoBehaviour
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 4.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 6.0f;
	[Tooltip("Rotation speed of the character")]
	public float RotationSpeed = 1.0f;
	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f;
	[Tooltip("Animation divider for the movement speed")]
	public float MoveSpeedAnimation = 6.0f;

	[Space(10)]
	[Tooltip("The maximum jump speed")]
	public float JumpSpeed = 1.2f;
	[Tooltip("Ramping of the jump speed")]
	public float JumpChangeRate = 0.1f;
	[Tooltip("Maximum time a jump can be held")]
	public float JumpDuration = 0.5f;
	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = 15.0f;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.1f;
	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	private float mTargetHorSpeed;
	private float mHorizontalSpeed;
	private float mTargetVerSpeed;
	private float mVerticalSpeed;
	private float mAnimationBlend;
	private float mTerminalVelocity = -53.0f;

	private float mJumpTimeoutDelta;
	private float mJumpDurationDelta;
	private float mFallTimeoutDelta;

	private bool mHeadingRight;
	
	private Character2DController mController;
	private CharacterSelector mSelector;
	private InputManager mInput;
	
    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        mController = GetComponent<Character2DController>();
        mSelector = GetComponent<CharacterSelector>();
        mInput = GetComponent<InputManager>();

        mTargetHorSpeed = 0.0f;
        mTargetVerSpeed = 0.0f;

        mJumpTimeoutDelta = JumpTimeout;
        mJumpDurationDelta = 0.0f;
        mFallTimeoutDelta = FallTimeout;

        mHeadingRight = true;
    }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    {
	    mTargetHorSpeed = mInput.sprint ? SprintSpeed : MoveSpeed;
	    if (mInput.move == Vector2.zero)
	    { mTargetHorSpeed = 0.0f; }
    }
    
    /// <summary>
    /// Update called at fixed intervals.
    /// </summary>
    void FixedUpdate ()
    {
	    MoveHorizontal();
	    JumpAndGravity();
	    AnimateCharacter();
	    
		var movement = new Vector3(
			mHorizontalSpeed * Math.Sign(mInput.move.x), 
			mVerticalSpeed, 
			0.0f
		);
		
	    mController.Move(movement * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Perform horizontal movement calculations.
    /// </summary>
    void MoveHorizontal()
    {
		var currentHorizontalSpeed = new Vector3(mController.velocity.x, 0.0f, mController.velocity.z).magnitude;

		var speedOffset = 0.1f;
		var inputMagnitude = mInput.analogMovement ? Math.Abs(mInput.move.x) : 1.0f;

		if (currentHorizontalSpeed < mTargetHorSpeed - speedOffset || 
		    currentHorizontalSpeed > mTargetHorSpeed + speedOffset)
		{
			mHorizontalSpeed = Mathf.Lerp(
				currentHorizontalSpeed, 
				mTargetHorSpeed * inputMagnitude, 
				Time.fixedDeltaTime * SpeedChangeRate
			);
			mHorizontalSpeed = Mathf.Round(mHorizontalSpeed * 1000f) / 1000f;
		}
		else
		{ mHorizontalSpeed = mTargetHorSpeed; }
    }
    
    /// <summary>
    /// Perform vertical movement calculations.
    /// </summary>
	private void JumpAndGravity()
	{
		if (mController.isGrounded)
		{
			mFallTimeoutDelta = FallTimeout;

			if (mInput.jump && mJumpTimeoutDelta <= 0.0f)
			{
				mTargetVerSpeed = Mathf.Sqrt(JumpSpeed * 2.0f * Gravity);
				mJumpTimeoutDelta = JumpTimeout;
				mJumpDurationDelta = JumpDuration; 
			}
			else
			{ mTargetVerSpeed = mVerticalSpeed; }
			
			if (mJumpTimeoutDelta >= 0.0f)
			{ mJumpTimeoutDelta -= Time.fixedDeltaTime; }
		}
		else
		{
			mTargetVerSpeed = mInput.jump && mJumpDurationDelta >= 0.0f
				? Mathf.Sqrt(JumpSpeed * 2.0f * Gravity)
				: mVerticalSpeed;
			
			if (mJumpDurationDelta >= 0.0f)
			{ mJumpDurationDelta -= Time.fixedDeltaTime; }
			
			if (mFallTimeoutDelta >= 0.0f)
			{ mFallTimeoutDelta -= Time.fixedDeltaTime; }
		}
		
		var currentVerticalSpeed = mController.velocity.y;
		
		var speedOffset = 0.1f;
		var inputMagnitude = 1.0f;

		if (currentVerticalSpeed < mTargetVerSpeed - speedOffset || 
			currentVerticalSpeed > mTargetVerSpeed + speedOffset)
		{
			mVerticalSpeed = Mathf.Lerp(
				currentVerticalSpeed, 
				mTargetVerSpeed * inputMagnitude, 
				Time.fixedDeltaTime * JumpChangeRate
			);
			mVerticalSpeed = Mathf.Round(mVerticalSpeed * 1000f) / 1000f;
		}
		else
		{ mVerticalSpeed = mTargetVerSpeed; }
		
		if (mVerticalSpeed > mTerminalVelocity)
		{ mVerticalSpeed -= Gravity * Time.fixedDeltaTime; }
	}

    /// <summary>
    /// Run animation according to the current state.
    /// </summary>
    void AnimateCharacter()
    {
	    if (mInput.move.x > 0 && !mHeadingRight)
		{
			// Flip to face right
			mHeadingRight = true;
			var localScale = transform.localScale;
			localScale.x = Math.Abs(localScale.x); // Ensure positive x-scale
			transform.localScale = localScale;
		}
		else if (mInput.move.x < 0 && mHeadingRight)
		{
			// Flip to face left
			mHeadingRight = false;
			var localScale = transform.localScale;
			localScale.x = -Math.Abs(localScale.x); // Ensure negative x-scale
			transform.localScale = localScale;
		}

	    var animator = mSelector.charAnimator;
	    if (animator != null)
	    {
			var currentVerticalSpeed = mController.velocity.y;
			var currentHorizontalSpeed = new Vector3(mController.velocity.x, 0.0f, mController.velocity.z).magnitude;
			
			// Property values: 
			var speed = currentHorizontalSpeed;
			var moveSpeed = Math.Abs(mTargetHorSpeed / MoveSpeedAnimation);
			var crouch = mInput.crouch;
			var grounded = mController.isGrounded;
			var jump = mInput.jump;
			var falling = !mController.isGrounded && mFallTimeoutDelta <= 0.0f;

			animator.SetFloat("Speed", speed);
			animator.SetFloat("MoveSpeed", moveSpeed);
			animator.SetBool("Jump", jump);
			animator.SetBool("Grounded", grounded);
			animator.SetBool("Fall", falling);
			animator.SetBool("Crouch", crouch);
	    }
    }
}

/*
 *    Copyright (C) by Upvoid Studios
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>
 */

using System;
using System.Collections.Generic;
using Engine;
using Engine.Universe;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Rendering;
using Engine.Physics;
using Engine.Input;

namespace UpvoidMiner
{
	/// <summary>
	/// Contains the game logic and the internal state of the player character.
	/// </summary>
	public class Player: EntityScript
	{
		/// <summary>
		/// The physical representation of the player. For now, this is a simple uncontrollable sphere.
		/// </summary>
		private PhysicsComponent physicsComponent;

		/// <summary>
		/// This is the camera that is used to show the perspective of the player.
		/// </summary>
		GenericCamera camera;

		/// <summary>
		/// This takes control of the rigid body attached to this entity and lets us walk around.
		/// </summary>
		CharacterController character;

        DiggingController digging;

        PlayerGui gui;

		// Define an area cube the user can NOT dig in.
		float halfCubeSideLength = 10f;

		// Position where this cube is located.
		vec3 currentAreaPosition = new vec3(0, 0, 0);

		// Create a pointer to a renderjob visualizing the area we are allowed to dig in.
		MeshRenderJob diggingConstraints = null;

        /// <summary>
        /// A list of items representing the inventory of the player.
        /// </summary>
        public List<Item> inventory = new List<Item>();

		public Player(GenericCamera _camera)
		{
			camera = _camera;
            Input.OnPressInput += HandlePressInput;
		}

        protected override void Init()
		{
            // Create a character controller that allows us to walk around.
            character = new CharacterController(camera, ContainingWorld);

			// For now, attach this entity to a simple sphere physics object.
			physicsComponent = new PhysicsComponent(thisEntity,
                                 character.Body,
			                     mat4.Translate(new vec3(0, 0.6f, 0)));

            physicsComponent.RigidBody.SetTransformation(mat4.Translate(new vec3(0, 30f, 0)));

            // This digging controller will perform digging and handle digging constraints for us.
            digging = new DiggingController(ContainingWorld);

            // Make the area we are allowed to dig in visible
            diggingConstraints = new MeshRenderJob(
                Renderer.Opaque.Mesh, 
                Resources.UseMaterial("DiggingConstraints", LocalScript.ModDomain), 
                Resources.UseMesh("::Debug/Box", LocalScript.ModDomain),
                mat4.Scale(0.999f * halfCubeSideLength)); // avoid z-fighting

            // Add this RenderJob to the world's jobs
            ContainingWorld.AddRenderJob(diggingConstraints);

            gui = new PlayerGui(this);

            AddTriggerSlot("AddItem");
		}

        void HandlePressInput (object sender, InputPressArgs e)
        {

            // Scale the area using + and - keys.
            // Translate it using up down left right (x, z)
            // and PageUp PageDown (y).
            if(e.PressType == InputPressArgs.KeyPressType.Down) {

				if ( e.Key == InputKey.F8 )
					Renderer.Opaque.Mesh.DebugWireframe = !Renderer.Opaque.Mesh.DebugWireframe;

                if(e.Key == InputKey.Plus) {
                    halfCubeSideLength += 0.5f;
                } else if(e.Key == InputKey.Minus) {
                    halfCubeSideLength -= 0.5f;
                } else if(e.Key == InputKey.Up) {
                    currentAreaPosition.z += 0.5f;
                } else if(e.Key == InputKey.Down) {
                    currentAreaPosition.z -= 0.5f;
                } else if(e.Key == InputKey.Left) {
                    currentAreaPosition.x += 0.5f;
                } else if(e.Key == InputKey.Right) {
                    currentAreaPosition.x -= 0.5f;
                } else if(e.Key == InputKey.PageUp) {
                    currentAreaPosition.y += 0.5f;
                } else if(e.Key == InputKey.PageDown) {
                    currentAreaPosition.y -= 0.5f;
                } else if(e.Key == InputKey.O) {
                    CsgExpression cube = new CsgExpression(1, "(max(max(abs(x - " + currentAreaPosition.x.ToString() + "), abs(y - " + currentAreaPosition.y.ToString() + ")), abs(z - " + currentAreaPosition.z.ToString() + ")) - " + halfCubeSideLength.ToString() + ")");
                    digging.SetConstraint(cube, new BoundingSphere(currentAreaPosition, halfCubeSideLength * 2f), DiggingController.ConstraintMode.OutsideAllowed);
                } else if(e.Key == InputKey.I) {
                    CsgExpression cube = new CsgExpression(1, "(max(max(abs(x - " + currentAreaPosition.x.ToString() + "), abs(y - " + currentAreaPosition.y.ToString() + ")), abs(z - " + currentAreaPosition.z.ToString() + ")) - " + halfCubeSideLength.ToString() + ")");
                    digging.SetConstraint(cube, new BoundingSphere(currentAreaPosition, halfCubeSideLength * 2f), DiggingController.ConstraintMode.InsideAllowed);
                }
            }

            // Set the new modelmatrix.
            diggingConstraints.ModelMatrix = mat4.Translate(currentAreaPosition) * mat4.Scale(0.999f * halfCubeSideLength);

            // We don't have tools or items yet, so we hard-code digging on left mouse click here.
            if((e.Key == InputKey.MouseLeft || e.Key == InputKey.MouseMiddle || e.Key == InputKey.C || e.Key == InputKey.V) && e.PressType == InputPressArgs.KeyPressType.Down) {

                // Send a ray query to find the position on the terrain we are looking at.
                ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * 0.5f, camera.Position + camera.ForwardDirection * 200f, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision) {
                    // Receiving the async ray query result here
                    if(_hit)
                    {
                        if (e.Key == InputKey.MouseLeft)
                        {
                            digging.DigSphere(_position, 1f);
                        } else if (e.Key == InputKey.MouseMiddle) {
                            digging.DigSphere(_position, 1f, 1, DiggingController.DigMode.Add);
                        }

                    }
                });
            }

            if(e.Key == InputKey.E && e.PressType == InputPressArgs.KeyPressType.Down) {
                ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * 0.5f, camera.Position + camera.ForwardDirection * 200f, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision) {
                    // Receiving the async ray query result here
                    if(_body != null)
                    {
                        Entity entity = _body.AttachedEntity;
                        if(entity != null)
                        {
                            TriggerId trigger = TriggerId.getIdByName("Interaction");
                            entity[trigger] |= new InteractionMessage(thisEntity);
                        }
                    }
                });
            }

        }

        /// <summary>
        /// This trigger slot is for sending an item to the receiving entity, which usually will add it to its inventory.
        /// This is triggered as a response to the Interaction trigger by items, but can be used whenever you want to give an item to a character.
        /// </summary>
        /// <param name="msg">Expected to be a of type PickupResponseMessage.</param>
        public void AddItem(object msg)
        {
            // Make sure we get the message type we are expecting.
            AddItemMessage addItemMsg = msg as AddItemMessage;
            if(addItemMsg == null)
                return;

            // Add the received item to the inventory.
            inventory.Add(addItemMsg.PickedItem);

        }

	}
}

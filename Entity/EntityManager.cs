namespace RCC.Entity
{
    public class EntityManager
    {
        private readonly RyzomClient _client;

        public UserEntity UserEntity;

        public EntityManager(RyzomClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Create an entity according to the slot and the form.
        /// </summary>
        /// <param name="slot">uint : slot for the entity</param> 
        /// <param name="form">uint32 : form to create the entity</param>
        /// <param name="newEntityInfo">dataset</param>
        /// <returns>CEntityCL : pointer on the new entity</returns> 
        public int Create(in byte slot, uint form, Change.TNewEntityInfo newEntityInfo)
        {
            _client.Automata.OnEntityCreate(slot, form, newEntityInfo);

            // TODO: Implementation

            return -1;
        }

        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <returns>bool : 'true' if the entity has been correctly removed</returns> 
        public bool Remove(in byte slot, bool warning)
        {
            _client.Automata.OnEntityRemove(slot, warning);

            // TODO: Implementation

            return true;
        }

        /// <summary>
        /// Method to update the visual property 'prop' for the entity in 'slot'.
        /// </summary>
        /// <param name="slot">uint : slot of the entity to update.</param> 
        /// <param name="prop">uint : the property to udapte.</param> 
        public void UpdateVisualProperty(in uint gameCycle, in byte slot, in byte prop, in uint predictedInterval)
        {
            _client.Automata.OnEntityUpdateVisualProperty(gameCycle, slot, prop, predictedInterval);

            // TODO: Implementation
        }

        /// <summary>
        /// Get an entity by name. Returns NULL if the entity is not found.
        /// </summary>
        /// <param name="name">of the entity to find</param>  
        /// <param name="caseSensitive">type of test to perform</param>  
        /// <param name="complete">if true, the name must match the full name of the entity</param>
        public Entity GetEntityByName(string name, bool caseSensitive, bool complete)
        {
            throw new System.NotImplementedException();
        }
    }
}
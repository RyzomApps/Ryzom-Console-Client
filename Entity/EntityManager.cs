namespace RCC.Entity
{
    internal class EntityManager
    {
        /// <summary>
        /// Create an entity according to the slot and the form.
        /// </summary>
        /// <param name="slot">uint : slot for the entity</param> 
        /// <param name="form">uint32 : form to create the entity</param>
        /// <param name="newEntityInfo">dataset</param>
        /// <returns>CEntityCL : pointer on the new entity</returns> 
        public int Create(in byte slot, uint form, Change.TNewEntityInfo newEntityInfo)
        {
            RyzomClient.GetInstance().GetLogger().Info("EntityManager.Create()");
            return -1;
        }

        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <returns>bool : 'true' if the entity has been correctly removed</returns> 
        public bool Remove(in byte slot, bool warning)
        {
            RyzomClient.GetInstance().GetLogger().Info("EntityManager.Remove()");

            return true;
        }

        /// <summary>
        /// Method to update the visual property 'prop' for the entity in 'slot'.
        /// </summary>
        /// <param name="slot">uint : slot of the entity to update.</param> 
        /// <param name="prop">uint : the property to udapte.</param> 
        public void UpdateVisualProperty(in uint gameCycle, in byte slot, in byte prop, in uint predictedInterval)
        {
            //RyzomClient.GetInstance().GetLogger().Info("EntityManager.UpdateVisualProperty()");
        }
    }
}
using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Reflection;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public abstract class InventoryTestBase : JiwaAPITest
    {
        protected async Task<InventoryItem> CreateInventoryItemAsync(string? description = null)
        {
            InventoryPOSTRequest createReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = description ?? ("Item " + RandomString(5)),
                DefaultPrice = 10.25M
            };

            InventoryItem createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            return createRes;
        }

        protected async Task DeleteInventoryItemAsync(string inventoryId)
        {
            await Client.DeleteAsync(new InventoryDELETERequest() { InventoryID = inventoryId });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }

        protected static string ReadString(object source, string propertyName)
        {
            PropertyInfo? prop = source.GetType().GetProperty(propertyName);
            return prop?.GetValue(source)?.ToString() ?? string.Empty;
        }

        protected static int ReadInt(object source, string propertyName)
        {
            PropertyInfo? prop = source.GetType().GetProperty(propertyName);
            object? value = prop?.GetValue(source);
            if (value is int i)
            {
                return i;
            }

            return int.TryParse(value?.ToString(), out int parsed) ? parsed : 0;
        }
    }
}


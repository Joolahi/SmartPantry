using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirebaseService
{
    private readonly FirestoreDb _firestoreDb;

    public FirebaseService()
    {
        var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
        var credentialsPath = "/etc/secrets/firebase-key.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

        _firestoreDb = FirestoreDb.Create(projectId);
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var productsRef = _firestoreDb.Collection("products");
        var snapshot = await productsRef.GetSnapshotAsync();

        var result = new List<ProductDto>();

        foreach (var doc in snapshot.Documents)
        {
            var data = doc.ToDictionary();

            if (data.Count == 0) continue;

            result.Add(new ProductDto
            {
                Name = data.GetValueOrDefault("name")?.ToString() ?? "",
                Barcode = data.GetValueOrDefault("barcode")?.ToString() ?? "",
                Brand = data.GetValueOrDefault("brand")?.ToString() ?? "",
                Energy = Convert.ToDouble(data.GetValueOrDefault("energy") ?? 0),
                BestBefore = data.GetValueOrDefault("bestBefore")?.ToString() ?? "",
                CreatedAt = data.GetValueOrDefault("createdAt") is Timestamp ts
                    ? ts.ToDateTime().ToString("o")
                    : "",
                Image = data.GetValueOrDefault("imageThumbUrl")?.ToString() ?? ""
            });
        }

        return result;
    }
}

using Google.Cloud.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.VisualBasic;
using Google.Cloud.Firestore.V1;
using System.Runtime.CompilerServices;

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

    public async Task<List<Dictionary<string, object>>> GetAllProductsAsync()
    {
        var productsRef = _firestoreDb.Collection("products");
        var snapshot = await productsRef.GetSnapshotAsync();

        var products = new List<Dictionary<string, object>>();
        foreach (var doc in snapshot.Documents)
        {
            products.Add(doc.ToDictionary());
        }
        return products;
    }
}
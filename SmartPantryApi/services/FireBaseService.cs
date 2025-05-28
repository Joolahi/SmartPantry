using Google.Cloud.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.VisualBasic;
using Google.Cloud.Firestore.V1;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            var raw = doc.ToDictionary();
            var cleaned = new Dictionary<string, object>();
            foreach (var entry in raw)
            {
                if (entry.Key == "createdAt" && entry.Value is Timestamp ts)
                {
                    cleaned[entry.Key] = ts.ToDateTime().ToString("o");
                }
                else
                {
                    cleaned[entry.Key] = entry.Value;
                }
            }
            if (cleaned.Count > 0)
            {
                products.Add(cleaned);
            }
        }
        return products;
    }
}
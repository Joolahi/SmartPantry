import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/product_model.dart';
import 'package:firebase_auth/firebase_auth.dart';

class ProductService {
  static Future<List<Product>> fetchProduct() async {
    final uid = FirebaseAuth.instance.currentUser?.uid;
    if (uid == null) {
      return [];
    }
    final String baseUrl =
        'https://smartpantry-backend.onrender.com/api/Products/$uid';
    try {
      final response = await http.get(Uri.parse('$baseUrl/firebase-products'));
      if (response.statusCode == 200) {
        final List<dynamic> data = json.decode(response.body);
        return data.map((item) => Product.fromJson(item)).toList();
      } else {
        throw Exception('Failed to load products');
      }
    } catch (e) {
      return [];
    }
  }
}

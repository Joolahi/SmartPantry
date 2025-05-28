import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/product_model.dart';

class ProductService {
  static const String _baseUrl =
      'https://smartpantry-backend.onrender.com/api/Products';

  static Future<List<Product>> fetchProduct() async {
    try {
      final response = await http.get(Uri.parse('$_baseUrl/firebase-products'));
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

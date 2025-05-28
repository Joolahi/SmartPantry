// product_list_page.dart
import 'package:flutter/material.dart';
import '../models/product_model.dart';
import '../services/product_service.dart';

class ProductListPage extends StatefulWidget {
  const ProductListPage({super.key});

  @override
  State<ProductListPage> createState() => _ProductListPageState();
}

class _ProductListPageState extends State<ProductListPage> {
  late Future<List<Product>> _futureProducts;

  @override
  void initState() {
    super.initState();
    _futureProducts = ProductService.fetchProduct();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: FutureBuilder<List<Product>>(
        future: _futureProducts,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          } else if (snapshot.hasError) {
            return Center(child: Text("Error: ${snapshot.error}"));
          } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(child: Text("No products found"));
          } else {
            final products = snapshot.data!;
            return ListView.builder(
              itemCount: products.length,
              itemBuilder: (context, index) {
                final p = products[index];
                return Column(
                  children: [
                    ListTile(
                      title: Text(p.name),
                      subtitle: Text("${p.brand} • ${p.energy} kcal"),
                      trailing: p.imageThumbUrl != null
                          ? Image.network(
                              p.imageThumbUrl!,
                              width: 50,
                              height: 50,
                              fit: BoxFit.cover,
                              errorBuilder: (context, error, stackTrace) {
                                return const Icon(Icons.broken_image, size: 50);
                              },
                            )
                          : const Icon(Icons.image),
                    ),
                    const Divider(
                      thickness: 0.8,
                      indent: 16,
                      endIndent: 16,
                      color: Color(0xFF2E7D32),
                    ),
                  ],
                );
              },
            );
          }
        },
      ),
    );
  }
}

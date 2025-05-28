class Product {
  final String name;
  final String brand;
  final double energy;
  final String createdAt;
  final String bestBefore;
  final String? imageThumbUrl;

  Product({
    required this.name,
    required this.brand,
    required this.energy,
    required this.createdAt,
    required this.bestBefore,
    this.imageThumbUrl,
  });

  factory Product.fromJson(Map<String, dynamic> json) {
    return Product(
      name: json['name'] ?? '',
      brand: json['brand'] ?? '',
      energy: (json['energy'] ?? 0).toDouble(),
      createdAt: json['createdAt'] ?? "",
      bestBefore: json['bestBefore'] ?? "",
      imageThumbUrl: json['imageThumbUrl'] ?? "",
    );
  }
}

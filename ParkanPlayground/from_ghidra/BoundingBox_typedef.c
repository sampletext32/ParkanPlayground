typedef struct BoundingBox {
    Vector3 BottomFrontLeft;
    Vector3 BottomFrontRight;
    Vector3 BottomBackRight;
    Vector3 BottomBackLeft;
    Vector3 TopBackRight;
    Vector3 TopFrontRight;
    Vector3 TopBackLeft;
    Vector3 TopFrontLeft;
} BoundingBox;
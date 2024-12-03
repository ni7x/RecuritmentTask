namespace RecuritmentTask.Tests
{
    public static class TestDataHelper
    {
        public static IEnumerable<object[]> GetInvalidData()
        {
            yield return new object[]
            {
            "", // Invalid title
            "Some description",
            "2025-12-05",
            0.5
            };

            yield return new object[]
            {
            "Valid Title",
            "400 character Description".PadRight(500, 'a'), // Invalid 500 characters long description
            "2025-12-05",
            1
            };

            yield return new object[]
            {
            "Valid Title",
            "Some description",
            "2023-12-01", // Invalid past expiry date
            0
            };

            yield return new object[]
            {
            "Valid Title",
            "Some description",
            "2025-12-05",
            1.2 // Invalid percentage (greater than 1)
            };
        }
    }
}

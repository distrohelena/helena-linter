class Test {
    void test() {
        int count = 1;

        int total = count + 1;

        System.out.println(total);

        switch (count) {
            case 1:
                int next = total + 1;

                System.out.println(next);
                break;
            default:
                break;
        }
    }
}

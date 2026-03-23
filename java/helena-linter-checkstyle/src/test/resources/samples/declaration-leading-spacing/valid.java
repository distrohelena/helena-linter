class Test {
    void test() {
        System.out.println("start");

        int count = 1;
        int total = count + 1;

        switch (count) {
            case 1:
                System.out.println("case");

                int next = total + 1;
                break;
            default:
                break;
        }
    }
}

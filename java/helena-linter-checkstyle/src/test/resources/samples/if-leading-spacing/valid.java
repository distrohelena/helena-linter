class Test {
    void test(boolean flag) {
        System.out.println("start");

        if (flag) {
            System.out.println(flag);
        }

        switch (String.valueOf(flag)) {
            case "true":
                System.out.println("case");

                if (flag) {
                    System.out.println("nested");
                }
                break;
            default:
                break;
        }
    }
}
